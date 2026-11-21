using Downloader;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Windows;

namespace ImageManager.ViewModels
{
    public class UpdateViewModel : PropertyChangedBase
    {
        public enum StateEnum { Checking, IsLatest, Updating, Error }
        public StateEnum State { get; private set; } = StateEnum.Checking;

        // 状态与对应图标
        public bool ShowCheckProgress => State == StateEnum.Checking;
        public bool ShowDownloadIcon => State == StateEnum.Updating;
        public bool ShowIsLastestIcon => State == StateEnum.IsLatest;
        public bool ShowErrorIcon => State == StateEnum.Error;

        /// <summary>
        /// 状态标题。
        /// </summary>
        public string StateText { get; private set; } = "正在检查更新…";
        /// <summary>
        /// 状态副标题，给当前版本等辅助信息用。
        /// </summary>
        public string StateSubText { get; private set; } = "";
        /// <summary>
        /// 异常原文，放在淡红卡片里。
        /// </summary>
        public string ErrorDetail { get; private set; } = "";

        public string CurrentVersion { get; } = App.AppVersion.ToString(3);
        public string LatestVersion { get; private set; } = "";

        /// <summary>
        /// 解析后的更新日志，每个版本一条。
        /// </summary>
        public BindableCollection<ReleaseNote> ReleaseNotes { get; } = new();

        /// <summary>
        /// 更新日志里包含的版本数。
        /// </summary>
        public int UpdateLogVersionCount { get; private set; }
        public string UpdateLogCountText =>
            UpdateLogVersionCount > 0 ? $"共 {UpdateLogVersionCount} 个版本" : string.Empty;
        public bool HasUpdateLog => UpdateLogVersionCount > 0;

        /// <summary>
        /// 日志区标题：有新版本看"更新内容"，已是最新看"更新历史"。
        /// </summary>
        public string LogTitle => State == StateEnum.Updating ? "更新内容" : "更新历史";

        /// <summary>
        /// 是否正在下载新版本（用于显示版本迁移行）。
        /// </summary>
        public bool HasNewVersion => State == StateEnum.Updating && !string.IsNullOrEmpty(LatestVersion);

        public bool ShowDownloadProgress { get; private set; }
        public double DownloadProgress { get; private set; }
        public string DownloadProgressPercentText => $"{DownloadProgress:0}%";

        public long ReceivedBytes { get; private set; }
        public long TotalBytes { get; private set; }
        public double BytesPerSecond { get; private set; }

        /// <summary>
        /// 下载进度文本：已下载 / 总量 · 剩余时间。
        /// </summary>
        public string DownloadProgressText
        {
            get
            {
                if (TotalBytes <= 0)
                    return string.Empty;
                var text = $"{FormatSize(ReceivedBytes)} / {FormatSize(TotalBytes)}";
                if (BytesPerSecond > 0 && ReceivedBytes < TotalBytes)
                {
                    var remain = (TotalBytes - ReceivedBytes) / BytesPerSecond;
                    text += $"　·　{FormatRemaining(remain)}";
                }
                return text;
            }
        }

        private string _downloadUrl;
        private CancellationTokenSource _cancellationTokenSource = new();

        /// <summary>
        /// 判断是否需要更新
        /// </summary>
        /// <returns></returns>
        public async Task<bool> NeedUpdateAsync()
        {
            try
            {
                using var client = GetGithubHttpClient();
                var response = await client.GetAsync("releases/latest");
                var version = App.AppVersion;
                var json = await response.Content.ReadAsStringAsync();
                var jsonObject = JObject.Parse(json);
                var latestVersion = new Version((string)jsonObject["tag_name"]);
                LatestVersion = latestVersion.ToString();
                if (latestVersion > version)
                {
                    _downloadUrl = (string)jsonObject["assets"][0]["browser_download_url"];
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                SetError("检查更新失败", "请检查网络或代理设置后再试", ex.Message);
            }
            return false;
        }

        public async Task LoadedAsync()
        {
            await Update();
        }

        public void Closed()
        {
            _cancellationTokenSource?.Cancel();
        }

        private HttpClient GetGithubHttpClient()
        {
            var url = "https://api.github.com/repos/nayaku/ImageManager/";
            var client = new HttpClient
            {
                BaseAddress = new Uri(url)
            };
            client.DefaultRequestHeaders.Add("Accept", "application/vnd.github+json");
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/64.0.3282.140 Safari/537.36 Edge/18.17763");
            client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");
            return client;
        }

        private async Task GetUpdateLogAsync(bool includeHistory)
        {
            var pageNum = 30;
            using var client = GetGithubHttpClient();
            for (var idx = 1; ; idx++)
            {
                var url = $"releases?page={idx}&&per_page={pageNum}";
                var response = await client.GetAsync(url);
                var json = await response.Content.ReadAsStringAsync();
                var jsonArray = JArray.Parse(json);
                foreach (var releasejson in jsonArray)
                {
                    var versionText = (string)releasejson["tag_name"];
                    var isCurrent = IsSameVersion(versionText);
                    var published_at = DateTime.Parse((string)releasejson["published_at"]);
                    ReleaseNotes.Add(new ReleaseNote
                    {
                        Version = versionText,
                        Date = published_at.ToString("yyyy年MM月dd日"),
                        IsNewest = ReleaseNotes.Count == 0 && !isCurrent,
                        IsCurrent = isCurrent,
                        Items = ParseNoteItems((string)releasejson["body"])
                    });
                    UpdateLogVersionCount = ReleaseNotes.Count;
                    // 有新版本时只关心"新版本 → 当前版本"这一段；已是最新时一直取到底
                    if (isCurrent && !includeHistory)
                        return;
                }

                if (jsonArray.Count < pageNum)
                    return;
            }
        }

        /// <summary>
        /// 判断远端版本号是否就是当前安装的版本。
        /// </summary>
        private static bool IsSameVersion(string versionText)
        {
            if (!Version.TryParse(versionText, out var version))
                return false;
            var current = App.AppVersion;
            return version.Major == current.Major
                && version.Minor == current.Minor
                && version.Build == current.Build;
        }

        private async Task Download()
        {

            if (!Directory.Exists("Update"))
                Directory.CreateDirectory("Update");
            var fileName = Path.Combine("Update", Path.GetFileName(_downloadUrl));
            if (File.Exists(fileName))
                File.Delete(fileName);

            // 判断是否能连接google
            bool canConnectGoogle;
            try
            {
                using var googleClient = new HttpClient
                {
                    Timeout = TimeSpan.FromSeconds(1)
                };
                using var googleResult = await googleClient.GetAsync("http://www.google.com");
                canConnectGoogle = googleResult.StatusCode == HttpStatusCode.OK;
            }
            catch (Exception)
            {
                canConnectGoogle = false;
            }

            // 选择从Github上下载还是从国内服务器下载
            string url = canConnectGoogle ?
                _downloadUrl :
                "http://im_installer.ngmks.com/" + Path.GetFileName(_downloadUrl);
            var downloadOpt = new DownloadConfiguration()
            {
                RequestConfiguration = new RequestConfiguration()
                {
                    Proxy = WebRequest.GetSystemWebProxy(),
                },
            };
            using var downloader = new DownloadService(downloadOpt);
            downloader.DownloadProgressChanged += (sender, e) =>
            {
                DownloadProgress = e.ProgressPercentage;
                ReceivedBytes = e.ReceivedBytesSize;
                TotalBytes = e.TotalBytesToReceive;
                BytesPerSecond = e.BytesPerSecondSpeed;
            };
            downloader.DownloadFileCompleted += (sender, e) =>
            {
                if (e.Error != null)
                {
                    SetError("下载更新失败", "请检查网络后重新打开本窗口", e.Error.Message);
                    return;
                }
                var p = new Process()
                {
                    StartInfo = new()
                    {
                        FileName = fileName,
                        UseShellExecute = true,
                        Verb = "runas",
                    }
                };
                p.Start();
                Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    Application.Current.Shutdown();
                });
            };
            await downloader.DownloadFileTaskAsync(url, fileName, _cancellationTokenSource.Token);
        }

        private async Task Update()
        {
            if (!await NeedUpdateAsync())
            {
                // 检查失败时不要被覆盖成"已是最新"
                if (State == StateEnum.Error)
                    return;
                State = StateEnum.IsLatest;
                StateText = "已是最新版本";
                // 已是最新也把历史拉下来，拉取失败不影响"已是最新"这个结论
                try
                {
                    await GetUpdateLogAsync(includeHistory: true);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
                StateSubText = ReleaseNotes.Count > 0
                    ? $"当前 {CurrentVersion}　·　最近更新 {ReleaseNotes[0].Date}"
                    : $"当前 {CurrentVersion}　·　无需更新";
                return;
            }
            try
            {
                await GetUpdateLogAsync(includeHistory: false);
                State = StateEnum.Updating;
                StateText = "正在下载更新…";
                ShowDownloadProgress = true;
                await Download();
            }
            catch (Exception ex)
            {
                SetError("下载更新失败", "请检查网络后重新打开本窗口", ex.Message);
            }
        }

        /// <summary>
        /// 统一切换到失败状态。
        /// </summary>
        private void SetError(string title, string hint, string detail)
        {
            State = StateEnum.Error;
            StateText = title;
            StateSubText = hint;
            ErrorDetail = detail;
        }

        /// <summary>
        /// 把 Markdown 正文拆成要点，忽略标题与空行。
        /// </summary>
        private static List<string> ParseNoteItems(string body) =>
            body.Split('\n')
                .Select(line => line.Trim().TrimStart('-', '*', '•').Trim())
                .Where(line => line.Length > 0 && !line.StartsWith('#'))
                .ToList();

        private static string FormatSize(long bytes) =>
            bytes >= 1024 * 1024
                ? $"{bytes / 1024d / 1024d:0.0} MB"
                : $"{bytes / 1024d:0} KB";

        private static string FormatRemaining(double seconds) =>
            seconds < 60
                ? $"剩余约 {seconds:0} 秒"
                : $"剩余约 {(int)(seconds / 60)} 分 {seconds % 60:0} 秒";

        /// <summary>
        /// 一条版本更新记录。
        /// </summary>
        public class ReleaseNote
        {
            public string Version { get; set; } = "";
            public string Date { get; set; } = "";
            /// <summary>
            /// 是否是本次要升级到的新版本。
            /// </summary>
            public bool IsNewest { get; set; }
            /// <summary>
            /// 是否是当前安装的版本。
            /// </summary>
            public bool IsCurrent { get; set; }
            public List<string> Items { get; set; } = new();
        }
    }
}
