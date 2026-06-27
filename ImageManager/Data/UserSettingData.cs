using HandyControl.Themes;
using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace ImageManager.Data
{
    public class UserSettingData : SettingsBase
    {
        private const string _settingDataFile = "UserSettings.xml";

        [XmlIgnore]
        public override string FilePath => _settingDataFile;

        #region 自定义设置区域
        /// <summary>
        /// 总存储路径
        /// </summary>
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
        public bool RestoreStickerOnStartup { get; set; } = true;
        public int ThumbnailWidth { get; set; } = 600;
        /// <summary>
        /// 卡片宽度
        /// </summary>
        public double CardWidth { get; set; } = 240;
        public ApplicationTheme Theme { get; set; } = ApplicationTheme.Light;
        public bool ClearUnUsedLabel { get; set; } = true;
        public enum OrderByEnum { AddTime, Title, AddState }
        public OrderByEnum OrderBy { get; set; } = OrderByEnum.AddTime;
        public bool IsDesc { get; set; } = true;
        public bool IsGroup { get; set; } = false;
        /// <summary>
        /// 图片相似度阈值
        /// </summary>
        public double SimilarityThreshold { get; set; } = 0.9;

        /// <summary>
        /// 截图时关闭主窗口
        /// </summary>
        public bool IsHideWhenScreenShoot { get; set; } = true;

        public List<string> WaitToDeleteFiles { get; set; }

        /// <summary>
        /// 每次加载图片数量
        /// </summary>
        public int TakePictureNumOneTime { get; set; } = 20;
        /// <summary>
        /// 第一次加载图片数量
        /// </summary>
        public int FirstLoadPictureNum { get; set; } = 50;

        /// <summary>
        /// 已打开贴片
        /// </summary>
        public ObservableCollection<string> Stickers { get; set; } = [];
        #endregion


        private static UserSettingData? _default = null;
        public static UserSettingData Default => _default 
            ??= Load<UserSettingData>(_settingDataFile) 
            ?? new UserSettingData();


        public UserSettingData()
        {
            // 创建目录
            if (!Directory.Exists(ImageFolderPath))
                Directory.CreateDirectory(ImageFolderPath);
            if (!Directory.Exists(TempFolderPath))
                Directory.CreateDirectory(TempFolderPath);
            if (!Directory.Exists(StickerFolderPath))
                Directory.CreateDirectory(StickerFolderPath);

            Stickers.CollectionChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(Stickers));
            };
        }
    }
}
