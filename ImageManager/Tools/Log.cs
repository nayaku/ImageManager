using System.Net.Http;
using System.Reflection;
using System.Text;

namespace ImageManager.Tools
{
    static class Log
    {
        /// <summary>
        /// 错误记录路径
        /// </summary>
        private static string LogPath = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "Log.txt");

        public static void Debug(string message) => Write("Debug", message);

        public static void Info(string message) => Write("Info", message);
        public static void Warn(string message) => Write("Warn", message);
        public static void Error(string message) => Write("Error", message);

        /// <summary>
        /// 写入错误
        /// </summary>
        private static void Write(string type, string message)
        {
            try
            {
                // 写入日志
                var sb = new StringBuilder();
                sb.AppendLine("*****************************************");
                sb.AppendLine(string.Format("{0}  {1} : {2}", DateTime.Now.ToLongDateString(), DateTime.Now.ToLongTimeString(), type));
                sb.AppendLine("");
                sb.AppendLine(message);
                sb.AppendLine("*****************************************");
                sb.AppendLine("");
                sb.AppendLine("");
                File.AppendAllText(LogPath, sb.ToString());
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }
        }


        /// <summary>
        /// 上传错误
        /// </summary>
        public static void ReportError(string error)
        {
            var machineName = Environment.MachineName;
            var userName = Environment.UserName;
            var version = Assembly.GetEntryAssembly().GetName().Version.ToString();
            var OSVersion = Environment.OSVersion.VersionString;
            var currentDirectory = Environment.CurrentDirectory;

            var formData = new MultipartFormDataContent
            {
                { new StringContent(machineName),"MachineName" },
                { new StringContent(userName),"UserName"},
                { new StringContent(version),"Version" },
                { new StringContent(OSVersion),"OSVersion" },
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
}
