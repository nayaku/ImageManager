using Downloader;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Windows;

namespace ImageManager.ViewModels
{
    public class UpdateViewModel : PropertyChangedBase
    {
        public enum StateEnum { Checking, IsLatest, Updating, Error }
        public StateEnum State { get; private set; } = StateEnum.Checking;
        public bool ShowCheckProgress => State == StateEnum.Checking || State == StateEnum.Updating;
        public bool ShowIsLastestIcon => State == StateEnum.IsLatest;
        public bool ShowErrorIcon => State == StateEnum.Error;

        public string StateText { get; private set; } = "";
        public string CurrentVersion { get; } = Assembly.GetEntryAssembly().GetName().Version.ToString(3);
        public string LatestVersion { get; private set; } = "";
        public string UpdateLog { get; private set; } = "";
        public bool ShowDownloadProgress { get; private set; }
        public double DownloadProgress { get; private set; }
        public string DownloadProgressText { get; private set; }

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
                var version = Assembly.GetEntryAssembly().GetName().Version;
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
                State = StateEnum.Error;
                StateText = ex.Message;
            }
            return false;
        }

        public void Loaded()
        {
            Update();
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

        private async Task GetUpdateLogAsync()
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
                    var version = (string)releasejson["tag_name"];
                    if (new Version(version) <= new Version(CurrentVersion))
                        return;
                    var published_at = DateTime.Parse((string)releasejson["published_at"]);
                    var body = (string)releasejson["body"];
                    UpdateLog += $"## {version}\n\n{published_at:yyyy年MM月dd日}\n\n{body}\n\n\n\n";
                }

                if (jsonArray.Count < 30)
                    return;
            }
        }

        private async void Download()
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
                var googleResult = await googleClient.GetAsync("http://www.google.com");
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
            };
            downloader.DownloadFileCompleted += (sender, e) =>
            {
                if (e.Error != null)
                {
                    State = StateEnum.Error;
                    StateText = e.Error.Message;
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

        private async void Update()
        {
            if (!await NeedUpdateAsync())
            {
                State = StateEnum.IsLatest;
                StateText = "已是最新版本";
                return;
            }
            try
            {
                await GetUpdateLogAsync();
                State = StateEnum.Updating;
                StateText = "正在下载更新...";
                ShowDownloadProgress = true;
                Download();
            }
            catch (Exception ex)
            {
                State = StateEnum.Error;
                StateText = ex.Message;
            }
        }
    }
}