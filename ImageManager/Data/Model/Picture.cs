using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Windows.Media.Imaging;

namespace ImageManager.Data.Model
{
    public struct Size
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public Size() { }
        public Size(int width, int height)
        {
            Width = width;
            Height = height;
        }
    }
    public enum PictureAddStateEnum
    {
        WaitToAdd,
        SameConflict,
        SimilarConflict
    }
    [Index(nameof(Hash), IsUnique = true)]
    public class Picture
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Title { get; set; }
        public bool HasTitleOrLabel => Title != null || Labels?.Count != 0;
        [Required]
        public string Path { get; set; }
        public string? ThumbnailPath { get; set; }

        public int Width { get; set; }
        public int Height { get; set; }
        public DateTime AddTime { get; set; }
        public string Hash { get; set; }
        public ulong? WeakHash { get; set; }
        public virtual List<Label> Labels { get; set; }

        [NotMapped]
        public bool AcceptToAdd { get; set; }
        [NotMapped]
        public List<Picture> SamePicture { get; set; }
        [NotMapped]
        public List<Picture> SimilarPictures { get; set; }
        [NotMapped]
        public PictureAddStateEnum AddState
        {
            get
            {
                if (SamePicture != null && SamePicture.Count > 0)
                    return PictureAddStateEnum.SameConflict;
                if (SimilarPictures != null && SimilarPictures.Count > 0)
                    return PictureAddStateEnum.SimilarConflict;
                return PictureAddStateEnum.WaitToAdd;
            }
        }
        [NotMapped]
        public string ImageFolderPath { get; set; }
        public void SetDefaultImageFolderPath()
        {
            var path = UserSettingData.Default.ImageFolderPath;
            if (!System.IO.Path.IsPathRooted(path))
                path = System.IO.Path.Join(AppDomain.CurrentDomain.BaseDirectory, path);
            ImageFolderPath = path;
        }
        public Picture()
        {
            SetDefaultImageFolderPath();
        }
        public Picture(bool setDefaultPath = true)
        {
            if (setDefaultPath)
                SetDefaultImageFolderPath();
        }
        //public Bitmap Bitmap
        //{
        //    get
        //    {
        //        var fileName = ThumbnailPath ?? Path;
        //        var filePath = System.IO.Path.Join(ImageFolderPath, fileName);
        //        var fif = FreeImageAPI.FREE_IMAGE_FORMAT.FIF_UNKNOWN;
        //        return FreeImageAPI.FreeImage.LoadBitmap(filePath, FreeImageAPI.FREE_IMAGE_LOAD_FLAGS.DEFAULT, ref fif);
        //    }
        //}
        public BitmapImage ImageSource
        {
            get
            {
                var fileName = ThumbnailPath ?? Path;
                var filePath = System.IO.Path.Join(ImageFolderPath, fileName);
                //var bi = new BitmapImage();
                //bi.BeginInit();
                //bi.CacheOption = BitmapCacheOption.OnLoad;
                //using (Stream ms = new MemoryStream(File.ReadAllBytes(filePath)))
                //{
                //    bi.StreamSource = ms;
                //    bi.EndInit();
                //    bi.Freeze();
                //}
                //return bi;
                return new BitmapImage(new Uri(filePath));
                //using var bitmap = Bitmap;
                //return ImageUtility.BitmapToImageSource(bitmap);
            }
        }
        public void CopyTo(string folderPath)
        {
            var filePath = System.IO.Path.Join(ImageFolderPath, Path);
            var newFilePath = System.IO.Path.Join(folderPath, Path);
            File.Copy(filePath, newFilePath);
            if (ThumbnailPath != null)
            {
                filePath = System.IO.Path.Join(ImageFolderPath, ThumbnailPath);
                newFilePath = System.IO.Path.Join(folderPath, ThumbnailPath);
                File.Copy(filePath, newFilePath);
            }
            ImageFolderPath = folderPath;
        }
    }
}
