using System.Net.Http;
using System.Reflection;

namespace ImageManager.Logging
{
    public class Logger
    {
        private readonly string _name;
        public Logger(string name)
        {
            _name = name;
        }
        /// <summary>
        /// 错误记录路径
        /// </summary>
        private readonly string LogPath = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "Logger.txt");

        public void Debug(string message) => Log(LogLevel.Debug, message);

        public void Info(string message) => Log(LogLevel.Info, message);
        public void Warn(string message) => Log(LogLevel.Warn, message);
        public void Error(Exception exception) => Log(LogLevel.Error, exception: exception);
        public void Error(string message) => Log(LogLevel.Error, message: message);
        public void Fatal(Exception exception) => Log(LogLevel.Fatal, exception: exception, report: true);
        public void Fatal(string message) => Log(LogLevel.Fatal, message: message, report: true);

        /// <summary>
        /// 写入错误
        /// <paramref name="level"/>错误等级
        /// <paramref name="message"/>错误信息
        /// <paramref name="exception"/>错误，如果有覆盖<paramref name="message"/>
        /// </summary>
        public void Log(LogLevel level, string message = "", Exception? exception = null, bool report = false)
        {
            if (exception != null)
                message = exception.Message;
            var datetime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz",
                System.Globalization.CultureInfo.InvariantCulture);

            // 控制台输出
            var logMessage = string.Format("{0}\t[{1}]:\t{2}({3})", level.ToString(),
                _name, message, datetime);
            System.Diagnostics.Debug.WriteLine(logMessage);

            // 写入日志
            if (level >= LogLevel.Info)
            {
                try
                {
                    File.AppendAllText(LogPath, logMessage);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e.ToString());
                }
            }
#if !DEBUG
            // 上传错误
            if (report)
                ReportError(level, exception != null ? exception.ToString() : message);
#endif
        }


        /// <summary>
        /// 上传错误
        /// </summary>
        public void ReportError(LogLevel level, string error)
        {
            var machineName = Environment.MachineName;
            var userName = Environment.UserName;
            var version = Assembly.GetEntryAssembly()!.GetName().Version!.ToString();
            var osVersion = Environment.OSVersion.VersionString;
            var currentDirectory = Environment.CurrentDirectory;

            var formData = new MultipartFormDataContent
            {
                { new StringContent(machineName),"MachineName" },
                { new StringContent(userName),"UserName"},
                { new StringContent(version),"Version" },
                { new StringContent(osVersion),"OSVersion" },
                { new StringContent(currentDirectory),"CurrentDirectory" },
                { new StringContent(error),"Error" },
            };
            try
            {
                using var client = new HttpClient();
                var severAddr = "https://im.ngmks.com";
                //#if DEBUG
                //                var severAddr = "http://127.0.0.1:5000";
                //#else
                //                var severAddr = "https://im.ngmks.com";
                //#endif
                var url = severAddr + "/api/report";
                var result = client.PostAsync(url, formData).Result;
                var respon = result.Content.ReadAsStringAsync().Result;
                System.Diagnostics.Debug.WriteLine(respon);
            }
            catch (Exception)
            {
                System.Diagnostics.Debug.WriteLine($"无法连接错误上报服务器。Error: {error}");
            }

        }
    }
    /// <summary>
    /// 错误级别
    /// </summary>
    public enum LogLevel
    {
        Debug,
        Info,
        Warn,
        Error,
        Fatal
    }
}
