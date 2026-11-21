namespace ImageManager.Settings
{
    public class ConfigurationData : SettingsBase
    {
        private const string SettingDataFile = "Configuration.xml";

        public override string FilePath => SettingDataFile;

        /// <summary>
        /// 每次加载图片数量
        /// </summary>
        public int TakePictureNumOneTime { get; set; } = 20;
        /// <summary>
        /// 第一次加载图片数量
        /// </summary>
        public int FirstLoadPictureNum { get; set; } = 50;

        private static ConfigurationData? _default = null;
        public static ConfigurationData Default => _default
            ??= Load<ConfigurationData>(SettingDataFile)
            ?? new ConfigurationData();
    }
}
