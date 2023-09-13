using ImageManager.Data.Model;
using Shipwreck.Phash;
using Shipwreck.Phash.Bitmaps;
using System.Collections.ObjectModel;
using System.Security.Cryptography;

namespace ImageManager.Data
{
    public class PictureFactory
    {
        private UserSettingData UserSettingData;
        private ImageContext _context;

        public PictureFactory(UserSettingData userSettingData, ImageContext context)
        {
            UserSettingData = userSettingData;
            _context = context;
        }

        /// <summary>
        /// 添加临时图片
        /// </summary>
        /// <param name="context"></param>
        /// <param name="filePath"></param>
        /// <param name="tempPictures"></param>
        /// <param name="deleteSourceFile">删除源文件</param>
        /// <returns></returns>
        /// <exception cref="ImageFormatException">文件格式不支持</exception>
        public Picture CreateTempPicture(string filePath, ReadOnlyCollection<Picture>? tempPictures = null,
            bool deleteSourceFile = false)
        {
            using var fs = File.OpenRead(filePath);
            using var reader = new BufferedStream(fs, 16 * 1024 * 1024); //16MB

            // 判断是否为可读格式
            var fif = FreeImageAPI.FreeImage.GetFileTypeFromStream(reader);
            if (fif == FreeImageAPI.FREE_IMAGE_FORMAT.FIF_UNKNOWN)
            {
                fif = FreeImageAPI.FreeImage.GetFIFFromFilename(filePath);
                if (fif == FreeImageAPI.FREE_IMAGE_FORMAT.FIF_UNKNOWN)
                {
                    throw new ImageFormatNotSupportException("图片格式不支持！");
                }
            }
            if (!FreeImageAPI.FreeImage.FIFSupportsReading(fif))
            {
                throw new ImageFormatNotSupportException("图片格式不支持！");
            }

            // 读取图片
            reader.Seek(0, SeekOrigin.Begin);
            var fibitmap = FreeImageAPI.FreeImage.LoadFromStream(reader, FreeImageAPI.FREE_IMAGE_LOAD_FLAGS.DEFAULT, ref fif);
            var width = (int)FreeImageAPI.FreeImage.GetWidth(fibitmap);
            var height = (int)FreeImageAPI.FreeImage.GetHeight(fibitmap);
            // 判断是否为位图
            if (FreeImageAPI.FreeImage.GetImageType(fibitmap) != FreeImageAPI.FREE_IMAGE_TYPE.FIT_BITMAP)
            {
                var newBitmap = FreeImageAPI.FreeImage.ConvertToType(fibitmap, FreeImageAPI.FREE_IMAGE_TYPE.FIT_BITMAP, true);
                FreeImageAPI.FreeImage.Unload(fibitmap);
                fibitmap = newBitmap;
            }
            // 转换为GDI+图片
            using var bitmap = FreeImageAPI.FreeImage.GetBitmap(fibitmap);
            FreeImageAPI.FreeImage.Unload(fibitmap);

            // 相同图片判断
            reader.Seek(0, SeekOrigin.Begin);
            var md5 = MD5.HashData(reader);
            var samePictures = new List<Picture>();
            var samePicture = _context.Pictures.Where(p => p.Hash.SequenceEqual(md5)).SingleOrDefault();
            if (samePicture != null)
                samePictures.Add(samePicture);
            if (tempPictures != null)
                samePictures.AddRange(tempPictures.Where(p => p.Hash == md5).ToList());

            // 相似判断
            var phash = ImagePhash.ComputeDigest(bitmap.ToLuminanceImage()).Coefficients;
            var similarPictures = _context.Pictures.AsEnumerable().Where(
                p => ImagePhash.GetCrossCorrelation(p.WeakHash, phash) > UserSettingData.SimilarityThreshold
                ).ToList();
            if (tempPictures != null)
                similarPictures.AddRange(tempPictures.Where(
                    p => ImagePhash.GetCrossCorrelation(p.WeakHash, phash) > UserSettingData.SimilarityThreshold
                    ).ToList());

            // 关闭文件，防止文件被占用
            reader.Close();
            fs.Close();
            // 保存到临时文件夹
            string saveFileName;
            do
            {
                saveFileName = $"{md5}{Random.Shared.Next()}";
            } while (File.Exists(Path.Join(UserSettingData.TempFolderPath, saveFileName + Path.GetExtension(filePath))));
            if (deleteSourceFile)
                File.Move(filePath, Path.Join(UserSettingData.TempFolderPath, saveFileName + Path.GetExtension(filePath)));
            else
                File.Copy(filePath, Path.Join(UserSettingData.TempFolderPath, saveFileName + Path.GetExtension(filePath)));

            string? thumbFileName = null;
            // 生成缩略图
            // 如果过宽或者格式不支持，生成缩略图
            if (width > UserSettingData.ThumbnailWidth || !(
                fif == FreeImageAPI.FREE_IMAGE_FORMAT.FIF_BMP
                || fif == FreeImageAPI.FREE_IMAGE_FORMAT.FIF_JPEG
                || fif == FreeImageAPI.FREE_IMAGE_FORMAT.FIF_PNG
                || fif == FreeImageAPI.FREE_IMAGE_FORMAT.FIF_TIFF
                || fif == FreeImageAPI.FREE_IMAGE_FORMAT.FIF_GIF
                || fif == FreeImageAPI.FREE_IMAGE_FORMAT.FIF_ICO
                || fif == FreeImageAPI.FREE_IMAGE_FORMAT.FIF_TIFF))
            {
                thumbFileName = saveFileName + "_s.png";
                if (width > UserSettingData.ThumbnailWidth)
                {
                    int targetWidth = UserSettingData.ThumbnailWidth;
                    int targetHeight = bitmap.Height * targetWidth / bitmap.Width;
                    using var thumb = bitmap.GetThumbnailImage(targetWidth, targetHeight, () => false, IntPtr.Zero); //ImageUtility.Resize(bitmap, targetWidth);
                    thumb.Save(Path.Join(UserSettingData.TempFolderPath, thumbFileName));
                }
                else
                {
                    bitmap.Save(Path.Join(UserSettingData.TempFolderPath, thumbFileName));
                }
            }

            // 判断是否接受
            var accept = false;
            if (samePictures.Count != 0)
                accept = false;
            else if (similarPictures.Count != 0)
                accept = false;// TODO 如果存在更加清晰的图片应该为更新替换，目前还没有更新替换功能
            else
                accept = true;

            // 生成Picture数据
            var dirPath = UserSettingData.Default.TempFolderPath;
            if (!Path.IsPathRooted(dirPath))
                dirPath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, dirPath);
            var picture = new Picture(false)
            {
                Title = Path.GetFileNameWithoutExtension(filePath),
                Path = saveFileName + Path.GetExtension(filePath),
                ThumbnailPath = thumbFileName,
                Width = width,
                Height = height,
                Hash = md5,
                WeakHash = phash,
                SamePicture = samePictures,
                SimilarPictures = similarPictures,
                ImageFolderPath = dirPath,
                AcceptToAdd = accept,
            };
            return picture;
        }

        public static bool IsPictureFile(string filePath)
        {
            var fif = FreeImageAPI.FreeImage.GetFileType(filePath, 0);
            if (fif == FreeImageAPI.FREE_IMAGE_FORMAT.FIF_UNKNOWN)
            {
                fif = FreeImageAPI.FreeImage.GetFIFFromFilename(filePath);
                if (fif == FreeImageAPI.FREE_IMAGE_FORMAT.FIF_UNKNOWN)
                {
                    return false;
                }
            }
            if (!FreeImageAPI.FreeImage.FIFSupportsReading(fif))
            {
                return false;
            }
            return true;
        }
    }

    ///<summary>
    ///图片格式不支持
    ///</summary>
    [Serializable]
    public class ImageFormatNotSupportException : Exception
    {
        public ImageFormatNotSupportException() { }
        public ImageFormatNotSupportException(string message) : base(message) { }
        public ImageFormatNotSupportException(string message, Exception inner) : base(message, inner) { }
        protected ImageFormatNotSupportException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
