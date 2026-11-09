using ImageManager.Tools;

namespace ImageManager.Logging
{
    public class Logger
    {
        private readonly string _name;
        public Logger(string name)
        {
            _name = name;
        }
        private static readonly object FileLock = new();

        public void Debug(string message) => Log(LogLevel.Debug, message);

        public void Info(string message) => Log(LogLevel.Info, message);
        public void Warn(string message) => Log(LogLevel.Warn, message);
        public void Error(Exception exception) => Log(LogLevel.Error, exception: exception);
        public void Error(string message) => Log(LogLevel.Error, message: message);
        public void Fatal(Exception exception) => Log(LogLevel.Fatal, exception: exception, report: true);
        public void Fatal(string message) => Log(LogLevel.Fatal, message: message, report: true);

        public static string GetLogPath(DateTime datetime) =>
            Path.Combine(AppPaths.LogDir, $"{Environment.ProcessId}_{datetime:yyyy-MM-dd}.log");

        /// <summary>
        /// 写入日志
        /// </summary>
        /// <param name="level">日志等级</param>
        /// <param name="message">日志信息</param>
        /// <param name="exception">异常；非空时覆盖 <paramref name="message"/></param>
        /// <param name="report">是否上传崩溃包，默认在后台线程上传</param>
        /// <param name="waitReport">同步等待上传完成；仅用于进程即将退出的场景</param>
        public void Log(LogLevel level, string message = "", Exception? exception = null,
            bool report = false, bool waitReport = false)
        {
            if (exception != null)
                message = exception.Message;
            var now = DateTime.Now;
            var datetime = now.ToString("yyyy-MM-ddTHH:mm:sszzz",
                System.Globalization.CultureInfo.InvariantCulture);

            // 控制台输出
            var logMessage = $"{level}\t[{_name}]:\t{message}({datetime})";
            System.Diagnostics.Debug.WriteLine(logMessage);

            // 写入日志
            if (level >= LogLevel.Info)
            {
                try
                {
                    lock (FileLock)
                    {
                        Directory.CreateDirectory(AppPaths.LogDir);
                        File.AppendAllText(GetLogPath(now), logMessage + Environment.NewLine);
                    }
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e.ToString());
                }
            }
#if !DEBUG
            // 上传错误
            if (report)
                Report.ReportCrash(level, message, exception, waitReport);
#endif
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
