using ImageManager.Tools;
using System.Reflection;

namespace ImageManager.ViewModels
{
    public class AboutViewModel : PropertyChangedBase
    {
        public string Version { get => Assembly.GetEntryAssembly().GetName().Version.ToString(3); }

        public string OnlineCount { get; set; } = "获取中…";

        public AboutViewModel()
        {
            LoadOnlineCountAsync();
        }

        private async void LoadOnlineCountAsync()
        {
            var count = await ServerApi.GetOnlineCountAsync();
            OnlineCount = count == null ? "获取失败" : $"{count} 台";
        }

        public void OpenProjectHome()
        {
            System.Diagnostics.Process.Start("explorer.exe", "https://github.com/nayaku/ImageManager");
        }

    }
}
