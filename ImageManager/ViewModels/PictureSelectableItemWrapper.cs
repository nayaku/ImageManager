using ImageManager.Data.Model;

namespace ImageManager.ViewModels
{

    public class PictureSelectableItemWrapper : PropertyChangedBase
    {
        private readonly bool _isAddPictureMode;

        public Picture Item { get; set; }
        public bool IsSelected { get; set; }

        public bool AcceptToAdd
        {
            get => Item.AcceptToAdd;
            set
            {
                if (Item.AcceptToAdd == value)
                    return;
                Item.AcceptToAdd = value;
                // 这两个属性由 Item 推导，Fody 侦测不到依赖，手动通知
                NotifyOfPropertyChange(nameof(IsRejected));
                NotifyOfPropertyChange(nameof(ContentOpacity));
            }
        }

        public bool IsEnabled => Item.AddState != PictureAddStateEnum.SameConflict;
        public bool IsConflict => Item.AddState != PictureAddStateEnum.WaitToAdd;

        /// <summary>
        /// 冲突角标文案：重复 / 相似。
        /// </summary>
        public string ConflictText => Item.AddState switch
        {
            PictureAddStateEnum.SameConflict => "重复",
            PictureAddStateEnum.SimilarConflict => "相似",
            _ => string.Empty
        };

        /// <summary>
        /// 添加模式下被标记为「不添加」的图片；冲突图不可添加，不算用户选择。
        /// </summary>
        public bool IsRejected => _isAddPictureMode && !IsConflict && !Item.AcceptToAdd;

        /// <summary>
        /// 不添加时整张卡片降到 55% 不透明度。
        /// </summary>
        public double ContentOpacity => IsRejected ? 0.55 : 1;

        public PictureSelectableItemWrapper(Picture item, bool isAddPictureMode = false)
        {
            Item = item;
            _isAddPictureMode = isAddPictureMode;
        }
    }
}
