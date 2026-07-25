using System.Xml.Serialization;

namespace ImageManager.Data
{
    /// <summary>
    /// 单张贴片的持久化状态（每张贴片一个 STMP\{GUID}.xml）。
    /// 只保存 DPI 无关量：缩放因子、源像素裁剪、DIP 坐标；显示尺寸在还原时按当前 DPI 重算。
    /// 继承 <see cref="SettingsBase"/>，任意属性变更即自动防抖存盘。
    /// </summary>
    public class StickerStateData : SettingsBase
    {
        [XmlIgnore]
        public override string FilePath =>
            Path.Join(UserSettingData.Default.StickerFolderPath, ImageFileName + ".xml");

        public string ImageFileName { get; set; } = string.Empty;
        public bool IsFromDatabase { get; set; }

        public double Left { get; set; }
        public double Top { get; set; }

        public double ZoomRate { get; set; } = 1.0;
        public double RotationAngle { get; set; }
        public bool IsFlippedH { get; set; }
        public bool IsFlippedV { get; set; }
        public double FlipScaleX => (IsFlippedH ? -1 : 1) * ZoomRate;
        public double FlipScaleY => (IsFlippedV ? -1 : 1) * ZoomRate;

        public double WindowOpacity { get; set; } = 1.0;
        public double EffectiveOpacity => IsFolded ? 1.0 : WindowOpacity;

        public bool IsFolded { get; set; }
        public int FoldCropX { get; set; }
        public int FoldCropY { get; set; }
        public double FoldOffsetX { get; set; }
        public double FoldOffsetY { get; set; }

        public StickerStateData() { }

        public StickerStateData(string imageFileName, bool isFromDatabase)
        {
            ImageFileName = imageFileName;
            IsFromDatabase = isFromDatabase;
        }
    }
}