using ImageManager.Logging;
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
        public byte[] Hash { get; set; }
        [Required]
        public byte[]? WeakHash { get; set; }
        public virtual List<Label> Labels { get; set; } = new();

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
                if (SamePicture?.Any() ?? false)
                    return PictureAddStateEnum.SameConflict;
                if (SimilarPictures?.Any() ?? false)
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

        public Uri ImageUri
        {
            get
            {
                var fileName = ThumbnailPath ?? Path;
                var filePath = System.IO.Path.Join(ImageFolderPath, fileName);
                return new Uri(filePath);
            }
        }

        public BitmapImage ImageSource
        {
            get
            {
                var fileName = ThumbnailPath ?? Path;
                var filePath = System.IO.Path.Join(ImageFolderPath, fileName);
                return new BitmapImage(ImageUri);
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

        public void SafeDeleteFile()
        {
            var logger = LoggerFactory.GetLogger(nameof(Picture));
            try
            {
                File.Delete(System.IO.Path.Join(ImageFolderPath, Path));
            }
            catch (Exception e)
            {
                logger.Error(e);
            }
            if (ThumbnailPath != null)
            {
                try
                {
                    File.Delete(System.IO.Path.Join(ImageFolderPath, ThumbnailPath));
                }
                catch (Exception e)
                {
                    logger.Error(e);
                }
            }
        }
    }
}
