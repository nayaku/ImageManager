using HandyControl.Themes;
using Stylet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ImageManager.Data
{
    public class UserSettingData : PropertyChangedBase
    {
        private static readonly string _settingDataFile = "UserSettings.xml";

        /// <summary>
        /// 自动保存设置
        /// </summary>
        [XmlIgnore]
        public bool AutoSave { get; set; } = true;

        /// <summary>
        /// 总存储路径
        /// </summary>
        public string StorePath { get; set; } = "SD";
        public string ImageFolderPath => Path.Join(StorePath, "IMG");
        public string TempFolderPath => Path.Join(StorePath, "TMP");
        public int ThumbnailWidth { get; set; } = 600;
        /// <summary>
        /// 卡片宽度
        /// </summary>
        public double CardWidth { get; set; } = 240;
        public ApplicationTheme Theme { get; set; } = ApplicationTheme.Light;
        public bool ClearUnUsedLabel { get; set; } = true;
        public enum OrderByEnum { AddTime, Title, AddState }
        public OrderByEnum OrderBy { get; set; } = OrderByEnum.AddTime;
        public bool IsDesc { get; set; } = false;
        public bool IsGroup { get; set; } = false;

        public List<String> WaitToDeleteFiles { get; set; }

        private static UserSettingData _default;
        public static UserSettingData Default => _default ??= Load();
        private System.Timers.Timer _saveUserSettingTimer;
        public UserSettingData()
        {
            _saveUserSettingTimer = new System.Timers.Timer(500)
            {
                AutoReset = false
            };
            _saveUserSettingTimer.Elapsed += (s, e) => Save();
            // 创建目录
            if (!Directory.Exists(ImageFolderPath))
                Directory.CreateDirectory(ImageFolderPath);
            if (!Directory.Exists(TempFolderPath))
                Directory.CreateDirectory(TempFolderPath);
        }

        /// <summary>
        /// 加载
        /// </summary>
        /// <returns></returns>
        public static UserSettingData Load()
        {
            if (File.Exists(_settingDataFile))
            {
                var serializer = new XmlSerializer(typeof(UserSettingData));
                using var stream = new FileStream(_settingDataFile, FileMode.Open);
                return serializer.Deserialize(stream) as UserSettingData;
            }
            else
            {
                return new UserSettingData();
            }
        }

        public void Save()
        {
            var writer = new XmlSerializer(typeof(UserSettingData));
            using var file = File.Create(_settingDataFile);
            writer.Serialize(file, this);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);
            if (AutoSave)
            {
                _saveUserSettingTimer.Stop();
                _saveUserSettingTimer.Start();
            }
        }
    }
}
