using System.Collections.ObjectModel;

namespace ImageManager.Settings
{
    public class AppStateData : SettingsBase
    {
        private const string SettingDataFile = "AppState.xml";

        public override string FilePath => SettingDataFile;

        /// <summary>
        /// 下次启动需要删除的图片
        /// </summary>
        public List<string>? WaitToDeleteFiles { get; set; }

        /// <summary>
        /// 已打开贴片
        /// </summary>
        public ObservableCollection<string> Stickers { get; set; } = [];

        private static AppStateData? _default = null;
        public static AppStateData Default => _default
            ??= Load<AppStateData>(SettingDataFile)
            ?? new AppStateData();

        public AppStateData()
        {
            Stickers.CollectionChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(Stickers));
            };
        }
    }
}
