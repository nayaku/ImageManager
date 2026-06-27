using ImageManager.Logging;
using System.ComponentModel;
using System.Xml.Serialization;
using Timer = System.Timers.Timer;

namespace ImageManager.Data
{
    /// <summary>
    /// SettingsBase是一个抽象类，用于管理应用程序的设置。
    /// 它提供了加载和保存设置的功能，并在属性更改时自动保存设置。
    /// </summary>
    public abstract class SettingsBase : PropertyChangedBase
    {
        private readonly object _lock = new();
        public abstract string FilePath { get; }

        /// <summary>
        /// 是否在属性变更时自动存盘。临时实例可置 false。
        /// </summary>
        [XmlIgnore]
        public bool AutoSave { get; set; } = true;

        private readonly Timer _saveUserSettingTimer;

        public SettingsBase()
        {
            _saveUserSettingTimer = new(800)
            {
                AutoReset = false
            };
            _saveUserSettingTimer.Elapsed += (s, e) => Save();

        }

        /// <summary>
        /// 加载配置文件
        /// </summary>
        /// <typeparam name="T">设置类的类型</typeparam>
        /// <param name="filePath">配置文件路径</param>
        /// <returns>类实例，如果加载失败则返回null</returns>
        public static T? Load<T>(string filePath) where T : SettingsBase
        {
            if (!File.Exists(filePath))
                return null;

            try
            {
                var xmlSerializer = new XmlSerializer(typeof(T));
                using var fs = File.OpenRead(filePath);
                var instance = (T)xmlSerializer.Deserialize(fs);
                return instance;
            }
            catch (Exception ex)
            {
                LoggerFactory.GetLogger(nameof(SettingsBase)).Error(ex);
                return null;
            }
        }

        /// <summary>
        /// 保存
        /// </summary>
        protected void Save()
        {
            try
            {
                var directory = Path.GetDirectoryName(FilePath)!;
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                lock (_lock)
                {
                    var xmlSerializer = new XmlSerializer(GetType());
                    using var fs = File.Create(FilePath);
                    xmlSerializer.Serialize(fs, this);
                }
            }
            catch (Exception ex)
            {
                LoggerFactory.GetLogger(nameof(SettingsBase)).Error(ex);
            }
        }

        /// <summary>
        /// 立刻保存设置
        /// </summary>
        public void Flush()
        {
            _saveUserSettingTimer.Stop();
            Save();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);
            if (!AutoSave)
                return;
            _saveUserSettingTimer.Stop();
            _saveUserSettingTimer.Start();
        }
    }
}
