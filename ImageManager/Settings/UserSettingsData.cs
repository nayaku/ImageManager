using System.ComponentModel.DataAnnotations;

namespace ImageManager.Settings
{
    public class UserSettingsData : SettingsBase
    {
        private const string SettingDataFile = "UserSettings.xml";

        public override string FilePath => SettingDataFile;

        /// <summary>
        /// 总存储路径
        /// </summary>
        [SettingsItem(DisplayName = "总存储路径", Description = "所有文件的总存储路径")]
        public string StorePath { get; set; } = "SD";
        public string ImageFolderPath => Path.Join(StorePath, "IMG");
        public string TempFolderPath => Path.Join(StorePath, "TMP");
        /// <summary>
        /// 贴片存储路径
        /// </summary>
        public string StickerFolderPath => Path.Join(StorePath, "STMP");
        /// <summary>
        /// 启动时还原上次打开的贴片
        /// </summary>
        [SettingsItem(DisplayName = "启动时还原上次打开的贴片", Description = "启动时是否还原上次打开的贴片")]
        public bool RestoreStickerOnStartup { get; set; } = true;

        /// <summary>
        /// 清理未使用标签
        /// </summary>
        [SettingsItem(DisplayName = "清理未使用标签", Description = "是否在启动时清理未使用的标签")]
        public bool ClearUnUsedLabel { get; set; } = true;

        /// <summary>
        /// 缩略图宽度
        /// </summary>
        [SettingsItem(DisplayName = "缩略图宽度", Description = "缩略图的宽度，单位为像素")]
        [Range(100, 2000, ErrorMessage = "缩略图宽度必须在100到2000之间")]
        public int ThumbnailWidth { get; set; } = 600;

        /// <summary>
        /// 图片相似度阈值
        /// </summary>
        [SettingsItem(DisplayName = "图片相似度阈值", Description = "图片相似度的阈值，范围在0到1之间")]
        [Range(0.0, 1.0, ErrorMessage = "图片相似度阈值必须在0到1之间")]
        public double SimilarityThreshold { get; set; } = 0.9;

        /// <summary>
        /// 截图时关闭主窗口
        /// </summary>
        [SettingsItem(DisplayName = "截图时关闭主窗口", Description = "截图时是否关闭主窗口")]
        public bool IsHideWhenScreenShoot { get; set; } = true;

        private static UserSettingsData? _default = null;
        public static UserSettingsData Default => _default
            ??= Load<UserSettingsData>(SettingDataFile)
            ?? new UserSettingsData();


        public UserSettingsData()
        {
            // 创建目录
            if (!Directory.Exists(ImageFolderPath))
                Directory.CreateDirectory(ImageFolderPath);
            if (!Directory.Exists(TempFolderPath))
                Directory.CreateDirectory(TempFolderPath);
            if (!Directory.Exists(StickerFolderPath))
                Directory.CreateDirectory(StickerFolderPath);
        }
    }
}
