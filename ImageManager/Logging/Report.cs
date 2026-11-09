using HandyControl.Controls;
using ImageManager.Tools;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.IO.Compression;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;

namespace ImageManager.Logging
{
    public static class Report
    {
        private const string ReportUrl = "http://im2.ngmks.com/api/v1/crashes/";
        private const int LogPackDays = 3;

        private static readonly Dictionary<(LogLevel, string, string?), DateTime> _lastReports = [];

        /// <summary>
        /// 打包并上传崩溃包；wait 为 false 时在线程池执行，不阻塞调用线程。
        /// 进程即将退出时须传 true，否则后台任务会被杀掉
        /// </summary>
        public static void ReportCrash(LogLevel level, string message, Exception? exception, bool wait)
        {
            if (!ShouldReport(level, message, exception))
                return;
            if (wait)
                DoReport(level, message, exception);
            else
                Task.Run(() => DoReport(level, message, exception));
        }

        /// <summary>
        /// 同一崩溃 10 分钟内只报一次
        /// </summary>
        private static bool ShouldReport(LogLevel level, string message, Exception? exception)
        {
            lock (_lastReports)
            {
                var key = (level, message, exception?.GetType().FullName);
                if (_lastReports.TryGetValue(key, out var lastReportTime)
                    && (DateTime.Now - lastReportTime).TotalMinutes < 10)
                    return false;
                _lastReports[key] = DateTime.Now;
                return true;
            }
        }

        /// <summary>
        /// 打包、上传，成功后删除 zip；任何情况下都不抛异常
        /// </summary>
        private static void DoReport(LogLevel level, string message, Exception? exception)
        {
            try
            {
                var zipPath = CreateCrashPackage(level, message, exception);
                if (zipPath == null)
                    return;
                var crashId = UploadCrashPackage(zipPath, level, message, exception);
                if (crashId != null)
                {
                    Notify(() => Growl.SuccessGlobal($"崩溃上报成功，ID: {crashId}"));
                    File.Delete(zipPath);
                }
            }
            catch (Exception e)
            {
                Notify(() => Growl.ErrorGlobal($"崩溃上报失败：{e}"));
            }
        }

        /// <summary>
        /// 切到 UI 线程显示通知；失败只记调试输出，保证上报路径不抛异常
        /// </summary>
        private static void Notify(Action show)
        {
            try
            {
                System.Windows.Application.Current?.Dispatcher.BeginInvoke(() =>
                {
                    try
                    {
                        show();
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine(e);
                    }
                });
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
            }
        }

        /// <summary>
        /// 打包 crash.txt 与最近日志，返回 zip 路径
        /// </summary>
        private static string? CreateCrashPackage(LogLevel level,
            string message, Exception? exception)
        {
            Directory.CreateDirectory(AppPaths.CrashDir);
            var now = DateTime.Now;
            var baseName = $"{Environment.ProcessId}_{now:yyyyMMddHHmmssfff}";
            var zipPath = Path.Combine(AppPaths.CrashDir, baseName + ".zip");

            try
            {
                using var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create);

                var infoEntry = zip.CreateEntry("crash.txt", CompressionLevel.SmallestSize);
                using (var writer = new StreamWriter(infoEntry.Open(), new UTF8Encoding(false)))
                    writer.Write(BuildCrashText(level, message, exception, now));
                for (var i = 0; i < LogPackDays; i++)
                {
                    var logPath = Logger.GetLogPath(now.AddDays(-i));
                    if (File.Exists(logPath))
                        AddFile(zip, logPath, Path.GetFileName(logPath));
                }
            }
            catch (Exception e)
            {
                Notify(() => Growl.ErrorGlobal($"打包崩溃包失败：{e}"));
                return null;
            }
            return zipPath;
        }

        /// <summary>
        /// 加入文件；日志可能正被写入，需共享读写打开
        /// </summary>
        private static void AddFile(ZipArchive zip, string path, string entryName)
        {
            using var src = new FileStream(path, FileMode.Open, FileAccess.Read,
                FileShare.ReadWrite | FileShare.Delete);
            using var dst = zip.CreateEntry(entryName, CompressionLevel.SmallestSize).Open();
            src.CopyTo(dst);
        }

        private static string BuildCrashText(LogLevel level, string message, Exception? exception, DateTime time)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Time: {time.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture)}");
            sb.AppendLine($"Level: {level.ToString()}");
            sb.AppendLine($"Version: {App.AppVersion}");
            sb.AppendLine($"OS: {Machine.GetOperatingSystem()} ({RuntimeInformation.OSArchitecture})");
            sb.AppendLine($"Runtime: {RuntimeInformation.FrameworkDescription}");
            sb.AppendLine($"Process: {Environment.ProcessId} ({RuntimeInformation.ProcessArchitecture})");
            sb.AppendLine($"WorkingSet: {Environment.WorkingSet / 1024 / 1024} MB");
            sb.AppendLine($"BaseDirectory: {AppDomain.CurrentDomain.BaseDirectory}");
            sb.AppendLine();
            sb.AppendLine(message);
            sb.AppendLine(exception?.ToString() ?? "");
            return sb.ToString();
        }


        private static int? UploadCrashPackage(string zipPath, LogLevel level,
            string message, Exception? exception)
        {
            try
            {
                using var fileStream = File.OpenRead(zipPath);
                using var fileStreamContent = new StreamContent(fileStream);
                fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue("application/zip");

                // 字段名与服务端 CrashCreate 一致，超长部分由服务端截断
                using var form = new MultipartFormDataContent
                {
                    { new StringContent(App.AppVersion.ToString()), "version" },
                    { new StringContent(level.ToString()), "level" },
                    { new StringContent(message), "error" },
                    { new StringContent(exception?.ToString() ?? ""), "callstack" },
                    { new StringContent(Environment.MachineName), "machine_name" },
                    { new StringContent(Machine.GetMachineCode()), "machine_code" },
                    { new StringContent(
                        $"{RuntimeInformation.OSDescription} ({RuntimeInformation.OSArchitecture})"),
                        "os" },
                    { fileStreamContent, "crash_file", Path.GetFileName(zipPath) },
                };

                using var client = new HttpClient()
                {
                    Timeout = TimeSpan.FromSeconds(5)
                };
                // HttpClient.Send 为真同步（.NET 5+），不经过 SynchronizationContext
                using var request = new HttpRequestMessage(HttpMethod.Post, ReportUrl) { Content = form };
                using var response = client.Send(request);
                using var reader = new StreamReader(response.Content.ReadAsStream());
                var responseContent = reader.ReadToEnd();
                if (!response.IsSuccessStatusCode)
                {
                    Notify(() => Growl.ErrorGlobal($"上传崩溃包失败：{responseContent}"));
                    return null;
                }
                var jsonObject = JObject.Parse(responseContent);
                var crashId = (int)jsonObject["id"]!;
                return crashId;
            }
            catch (Exception e)
            {
                Notify(() => Growl.ErrorGlobal($"无法连接错误上报服务器：{e.Message}"));
                return null;
            }
        }
    }
}
