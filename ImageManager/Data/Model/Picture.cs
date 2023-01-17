using ImageManager.Tools;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.IO;
using System.Windows.Media;
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
    public enum PictureType
    {
        // 本地图片
        LocalPicture,
        // 截图
        ScreenShot,
        // 剪贴板
        Clipboard,
        // 网络图片
        WebPicture,
    }

    [Index(nameof(Hash), IsUnique = true)]
    public class Picture
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string? Title { get; set; }
        public bool HasTitleOrLabel => Title != null || Labels?.Count != 0;
        [Required]
        public string Path { get; set; }
        public string? ThumbnailPath { get; set; }

        public int Width { get; set; }
        public int Height { get; set; }
        public DateTime AddTime { get; set; }
        public PictureType Type { get; set; }
        public string? Hash { get; set; }
        public ulong? WeakHash { get; set; }
        public virtual List<Label> Labels { get; set; }

        [NotMapped]
        public bool IsSelected { get; set; }
        [NotMapped]
        public List<Picture> SamePicture { get; set; }
        [NotMapped]
        public List<Picture> SimilarPictures { get; set; }
        [NotMapped]
        public static string ImageFolderPath { get; }
        static Picture()
        {
            var path = UserSettingData.Instance.ImageFolderPath;
            if (!System.IO.Path.IsPathRooted(path))
                path = System.IO.Path.Join(AppDomain.CurrentDomain.BaseDirectory, path);
            ImageFolderPath = path;
        }
        public Bitmap Bitmap
        {
            get
            {
                var fileName = ThumbnailPath ?? Path;
                var filePath = System.IO.Path.Join(ImageFolderPath, fileName);
                var fif = FreeImageAPI.FREE_IMAGE_FORMAT.FIF_UNKNOWN;
                return FreeImageAPI.FreeImage.LoadBitmap(filePath, FreeImageAPI.FREE_IMAGE_LOAD_FLAGS.DEFAULT, ref fif);
            }
        }
        public BitmapImage ImageSource
        {
            get
            {
                using var bitmap = Bitmap;
                return ImageUtility.BitmapToImageSource(bitmap);
            }
        }
        public void DeleteFile()
        {
            var filePath = System.IO.Path.Join(ImageFolderPath, Path);
            if (File.Exists(filePath))
                File.Delete(filePath);
            if (ThumbnailPath != null)
            {
                filePath = System.IO.Path.Join(ImageFolderPath, ThumbnailPath);
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
        }
    }
}
