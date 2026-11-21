using HandyControl.Themes;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Media;

namespace ImageManager.Themes
{
    /// <summary>
    /// 主题模式。
    /// </summary>
    public enum ThemeMode
    {
        Light,
        Dark,
        System
    }

    /// <summary>
    /// 主题与强调色管理。
    /// 明暗切换交给 HandyControl，配色令牌由 <see cref="AppPalette" /> 同步。
    /// </summary>
    public static class AppTheme
    {
        /// <summary>
        /// 可选强调色数量。
        /// </summary>
        public const int AccentCount = 6;

        private static bool _systemEventHooked;
        private static ThemeMode _mode = ThemeMode.System;

        /// <summary>
        /// 当前实际生效的明暗主题。
        /// </summary>
        public static ApplicationTheme EffectiveTheme => ThemeManager.Current.ActualApplicationTheme;

        /// <summary>
        /// 取强调色，越界时回落到第一种。
        /// 颜色定义在 Themes/Tokens.Common.xaml 的 AppAccent0..5。
        /// </summary>
        public static Color GetAccentColor(int index)
        {
            if (index < 0 || index >= AccentCount)
                index = 0;
            if (Application.Current?.TryFindResource($"AppAccent{index}") is SolidColorBrush brush)
                return brush.Color;
            return Color.FromRgb(0x3B, 0x7B, 0xF6);
        }

        /// <summary>
        /// 应用主题模式与强调色。
        /// </summary>
        public static void Apply(ThemeMode mode, int accentIndex)
        {
            _mode = mode;
            ApplicationTheme target;
            switch (mode)
            {
                case ThemeMode.Light:
                    ThemeManager.Current.UsingSystemTheme = false;
                    ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
                    target = ApplicationTheme.Light;
                    break;
                case ThemeMode.Dark:
                    ThemeManager.Current.UsingSystemTheme = false;
                    ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
                    target = ApplicationTheme.Dark;
                    break;
                default:
                    ThemeManager.Current.UsingSystemTheme = true;
                    // 跟随系统时以 HandyControl 解析出的实际主题为准
                    target = EffectiveTheme;
                    break;
            }

            // 跟随系统时强调色由系统决定，不再下发自定义色
            if (mode != ThemeMode.System)
                ApplyAccent(accentIndex);
            AppPalette.Apply(target);
            HookSystemTheme();
        }

        /// <summary>
        /// 应用强调色。
        /// </summary>
        public static void ApplyAccent(int accentIndex)
        {
            var brush = new SolidColorBrush(GetAccentColor(accentIndex));
            brush.Freeze();
            ThemeManager.Current.AccentColor = brush;
        }

        /// <summary>
        /// 跟随系统时监听系统主题变化。
        /// </summary>
        private static void HookSystemTheme()
        {
            if (_systemEventHooked)
                return;
            _systemEventHooked = true;
            SystemEvents.UserPreferenceChanged += (_, _) =>
            {
                if (_mode == ThemeMode.System)
                    AppPalette.Apply(EffectiveTheme);
            };
        }
    }
}
