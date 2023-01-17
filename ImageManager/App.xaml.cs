using HandyControl.Themes;
using System.Windows;
using System.Windows.Threading;

namespace ImageManager
{
    public partial class App : Application
    {
        internal void UpdateTheme(ApplicationTheme theme)
        {
            if (ThemeManager.Current.ApplicationTheme != theme)
            {
                ThemeManager.Current.ApplicationTheme = theme;
            }
        }
    }
}
