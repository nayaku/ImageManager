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
        /// 总存储路径
        /// </summary>
        public string StorePath { get; set; } = "SD";
        public string ImageFolderPath => Path.Join(StorePath, "IMG");
        public string TempFolderPath => Path.Join(TempFolderPath, "TMP");
        /// <summary>
        /// 卡片宽度
        /// </summary>
        public double CardWidth { get; set; } = 240;
        public ApplicationTheme Theme { get; set; } = ApplicationTheme.Light;
        public bool ClearUnUsedLabel { get; set; } = true;

        private static UserSettingData _instance;
        public static UserSettingData Instance => _instance ??= Load();

        /// <summary>
        /// 加载
        /// </summary>
        /// <returns></returns>
        private static UserSettingData Load()
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
    }
}
