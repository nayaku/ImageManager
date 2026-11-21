using ImageManager.Themes;

namespace ImageManager.Settings
{
    /// <summary>
    /// 界面上的可变设置数据
    /// </summary>
    public class ViewSettingsData : SettingsBase
    {
        private const string SettingDataFile = "ViewSettings.xml";

        public override string FilePath => SettingDataFile;

        /// <summary>
        /// 主题模式（明 / 暗 / 跟随系统）
        /// </summary>
        public ThemeMode ThemeMode { get; set; } = ThemeMode.System;
        /// <summary>
        /// 强调色序号，对应 Themes/Tokens.Common.xaml 中的 AppAccent0..5
        /// </summary>
        public int AccentIndex { get; set; } = 0;
        /// <summary>
        /// 卡片宽度
        /// </summary>
        public double CardWidth { get; set; } = 240;

        public enum OrderByEnum { AddTime, Title, AddState }
        public OrderByEnum OrderBy { get; set; } = OrderByEnum.AddTime;
        public bool IsDesc { get; set; } = true;
        public bool IsGroup { get; set; } = false;

        private static ViewSettingsData? _default = null;
        public static ViewSettingsData Default => _default
            ??= Load<ViewSettingsData>(SettingDataFile)
            ?? new ViewSettingsData();
    }
}
