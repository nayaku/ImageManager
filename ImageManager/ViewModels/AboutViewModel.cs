using System.Reflection;

namespace ImageManager.ViewModels
{
    public class AboutViewModel : PropertyChangedBase
    {
        public string Version { get => Assembly.GetEntryAssembly().GetName().Version.ToString(3); }

        public void OpenProjectHome()
        {
            System.Diagnostics.Process.Start("explorer.exe", "https://github.com/nayaku/ImageManager");
        }

    }
}