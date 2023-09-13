using ImageManager.Data.Model;

namespace ImageManager.ViewModels
{

    public class PictureSelectableItemWrapper : PropertyChangedBase
    {

        public Picture Item { get; set; }
        public bool IsSelected { get; set; }
        public bool AcceptToAdd { get => Item.AcceptToAdd; set => Item.AcceptToAdd = value; }
        public bool IsEnabled => Item.AddState != PictureAddStateEnum.SameConflict;
        public bool IsConflict => Item.AddState != PictureAddStateEnum.WaitToAdd;
        public PictureSelectableItemWrapper(Picture item)
        {
            Item = item;
        }
    }
}
