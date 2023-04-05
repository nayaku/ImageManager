using ImageManager.Data.Model;

namespace ImageManager.ViewModels
{

    public class PictureSelectableItemWrapper : PropertyChangedBase
    {

        public Picture Item { get; set; }
        public bool IsSelected { get; set; }
        public bool AcceptToAdd { get => Item.AcceptToAdd; set => Item.AcceptToAdd = value; }
        public bool IsEnabled => !(Item.SamePicture != null && Item.SamePicture.Count > 0);
        public bool IsConflict => (Item.SamePicture != null && Item.SamePicture.Count > 0)
            || (Item.SimilarPictures != null && Item.SimilarPictures.Count > 0);
        public PictureSelectableItemWrapper(Picture item)
        {
            Item = item;
        }
    }
}
