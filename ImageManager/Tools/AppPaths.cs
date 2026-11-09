namespace ImageManager.Tools
{
    public static class AppPaths
    {
        /// <summary>
        /// 用户数据根目录：%LocalAppData%\ImageManager
        /// </summary>
        public static readonly string DataDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData,
                Environment.SpecialFolderOption.Create),
            "ImageManager");

        /// <summary>
        /// 日志目录
        /// </summary>
        public static readonly string LogDir = Path.Combine(DataDir, "Logs");

        /// <summary>
        /// 崩溃包目录
        /// </summary>
        public static readonly string CrashDir = Path.Combine(DataDir, "Crashes");
    }
}
