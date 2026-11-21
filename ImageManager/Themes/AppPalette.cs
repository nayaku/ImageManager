using HandyControl.Themes;
using System.Windows;
using System.Windows.Media;

namespace ImageManager.Themes
{
    /// <summary>
    /// 界面配色令牌。
    /// 切主题时整块替换资源值，DynamicResource 引用会自动刷新；
    /// 注意不能复用画刷实例，ResourceDictionary 会冻结放进去的 Freezable。
    /// </summary>
    public static class AppPalette
    {
        /// <summary>
        /// 注册默认（浅色）令牌，需在窗口解析资源前调用。
        /// </summary>
        public static void Register() => Apply(ApplicationTheme.Light);

        /// <summary>
        /// 按明暗主题刷新令牌颜色。
        /// </summary>
        public static void Apply(ApplicationTheme theme)
        {
            var resources = Application.Current?.Resources;
            if (resources == null)
                return;
            var dark = theme == ApplicationTheme.Dark;
            resources["AppPageBrush"] = Brush(dark ? "#1B1D21" : "#F6F7F9");
            resources["AppCardBrush"] = Brush(dark ? "#23262B" : "#FFFFFF");
            resources["AppSurfaceBrush"] = Brush(dark ? "#2C3037" : "#F0F2F5");
            resources["AppStrokeBrush"] = Brush(dark ? "#333842" : "#E7EAF0");
            resources["AppImageBackdropBrush"] = Brush(dark ? "#1E2126" : "#F1F3F6");
        }

        private static SolidColorBrush Brush(string hex)
        {
            var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
            brush.Freeze();
            return brush;
        }
    }
}
