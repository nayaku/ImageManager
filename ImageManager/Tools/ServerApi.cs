using ImageManager.Logging;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;

namespace ImageManager.Tools
{
    /// <summary>
    /// 服务端接口：机器心跳与在线人数查询
    /// </summary>
    public static class ServerApi
    {
        public const string BaseUrl = "http://imapi.ngmks.com/api/v1/";

        /// <summary>
        /// 心跳间隔，须小于服务端 1 小时的会话窗口
        /// </summary>
        private static readonly TimeSpan HeartbeatInterval = TimeSpan.FromMinutes(30);

        private static readonly HttpClient _client = new() { Timeout = TimeSpan.FromSeconds(5) };
        private static readonly Logger _logger = LoggerFactory.GetLogger(nameof(ServerApi));
        private static int _heartbeatStarted;

        /// <summary>
        /// 启动后台心跳
        /// </summary>
        public static void StartHeartbeat()
        {
            if (Interlocked.Exchange(ref _heartbeatStarted, 1) == 1)
                return;
            Task.Run(async () =>
            {
                await SendHeartbeatAsync();
                using var timer = new PeriodicTimer(HeartbeatInterval);
                while (await timer.WaitForNextTickAsync())
                    await SendHeartbeatAsync();
            });
        }

        /// <summary>
        /// 上报机器信息
        /// </summary>
        private static async Task SendHeartbeatAsync()
        {
            try
            {
                var body = new JObject
                {
                    ["version"] = App.AppVersion.ToString(),
                    ["machine_name"] = Environment.MachineName,
                    ["machine_code"] = Machine.GetMachineCode(),
                    ["os"] = $"{RuntimeInformation.OSDescription} ({RuntimeInformation.OSArchitecture})",
                };
                using var content = new StringContent(body.ToString(), Encoding.UTF8, "application/json");
                using var response = await _client.PostAsync(BaseUrl + "machines/", content);
                if (!response.IsSuccessStatusCode)
                    _logger.Warn($"心跳上报失败：{(int)response.StatusCode} {await response.Content.ReadAsStringAsync()}");
            }
            catch (Exception e)
            {
                _logger.Warn($"心跳上报失败：{e.Message}");
            }
        }

        /// <summary>
        /// 获取当前在线机器数；失败返回 null
        /// </summary>
        public static async Task<int?> GetOnlineCountAsync()
        {
            try
            {
                using var response = await _client.GetAsync(BaseUrl + "machines/current");
                var text = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode && int.TryParse(text, out var count))
                    return count;
                _logger.Warn($"获取在线人数失败：{(int)response.StatusCode} {text}");
            }
            catch (Exception e)
            {
                _logger.Warn($"获取在线人数失败：{e.Message}");
            }
            return null;
        }
    }
}
