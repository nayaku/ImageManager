using ImageManager.Data;
using ImageManager.Data.Model;
using ImageManager.Tools;
using Stylet;
using StyletIoC;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

namespace ImageManager.ViewModels
{
    public class ProgressViewModel : Screen
    {
        [Inject]
        public UserSettingData UserSettingData { get; set; }
        [Inject]
        public ImageContext Context { get; set; }

        public double Progress { get; set; }
        public string Message { get; set; }
        public string Title => string.Format("{0}中...({1:F2}%)", _isDoingCleanUp ? "回退" : "处理", Progress);
        public List<string> Files { get; private set; } = new();
        public List<Picture> Pictures { get; private set; } = new();

        private bool _isDoingCleanUp;
        private CancellationTokenSource _cancellationTokenSource = new();
        private List<string> _dirFiles;
        private bool _canClose = true;

        public ProgressViewModel(List<string> dirFiles)
        {
            if (!Directory.Exists(UserSettingData.ImageFolderPath))
                Directory.CreateDirectory(UserSettingData.ImageFolderPath);
            if (!Directory.Exists(UserSettingData.TempFolderPath))
                Directory.CreateDirectory(UserSettingData.TempFolderPath);
            _dirFiles = dirFiles;
        }

        public override Task<bool> CanCloseAsync()
        {
            return Task.Run(() =>
            {
                if (_canClose == false && _cancellationTokenSource.Token.IsCancellationRequested == false)
                    CancelTask();
                return _canClose;
            });
        }

        public void CancelTask()
        {
            _cancellationTokenSource.Cancel();
        }

        /// <summary>
        /// 做取消清理工作
        /// </summary>
        private void CleanTemp()
        {
            _isDoingCleanUp = true;
            var preProgress = Progress;
            foreach (var picture in Pictures)
            {
                if (picture.Path != null)
                {
                    var path = Path.Join(UserSettingData.TempFolderPath, picture.Path);
                    Message = $"删除文件{path}";
                    File.Delete(path);
                }
                if (picture.ThumbnailPath != null)
                {
                    var path = Path.Join(UserSettingData.TempFolderPath, picture.ThumbnailPath);
                    Message = $"删除文件{path}";
                    File.Delete(path);
                }
                Progress -= 1.0 / Pictures.Count * preProgress * 100;
            }
        }
        public async void DoTask()
        {
            var result = await Task.Run(() =>
            {
                _canClose = false;
                Message = "正在准备处理...";
                foreach (var dirFile in _dirFiles)
                {
                    if (_cancellationTokenSource.Token.IsCancellationRequested)
                        return false;
                    Walk(dirFile);
                }
                Progress = 20;
                foreach (var file in Files)
                {
                    if (_cancellationTokenSource.Token.IsCancellationRequested)
                        return false;
                    Message = "正在处理{file}...";
                    ProcessFile(file);
                    Progress += 1.0 / Files.Count * 80;
                }
                if (_cancellationTokenSource.Token.IsCancellationRequested)
                    return false;
                return true;
            }, _cancellationTokenSource.Token);
            // 被取消
            if (!result)
            {
                CleanTemp();
            }
            // 准备返回
            _canClose = true;
            RequestClose(result);
        }

        private void Walk(string path)
        {
            if (_cancellationTokenSource.Token.IsCancellationRequested)
                return;
            if (File.Exists(path))
                Files.Add(path);
            Progress = Math.Min(Progress + 0.5, 20);
            Message = $"正在遍历文件夹{path}...";
            var directories = Directory.GetDirectories(path);
            foreach (var directory in directories)
            {
                Walk(directory);
            }
            var files = Directory.GetFiles(path);
            Files.AddRange(files);
        }

        private void ProcessFile(string file)
        {

            var FileStream = File.OpenRead(file);
            var reader = new BufferedStream(FileStream, 4 * 1024 * 1024);

            // 判断是否为可读格式
            var fif = FreeImageAPI.FreeImage.GetFileTypeFromStream(reader);
            if (fif == FreeImageAPI.FREE_IMAGE_FORMAT.FIF_UNKNOWN)
            {
                fif = FreeImageAPI.FreeImage.GetFIFFromFilename(file);
                if (fif == FreeImageAPI.FREE_IMAGE_FORMAT.FIF_UNKNOWN)
                {
                    return;
                }
            }
            if (!FreeImageAPI.FreeImage.FIFSupportsReading(fif))
            {
                return;
            }

            // 读取图片
            reader.Seek(0, SeekOrigin.Begin);
            var fibitmap = FreeImageAPI.FreeImage.LoadFromStream(reader, FreeImageAPI.FREE_IMAGE_LOAD_FLAGS.DEFAULT, ref fif);
            var width = (int)FreeImageAPI.FreeImage.GetWidth(fibitmap);
            var height = (int)FreeImageAPI.FreeImage.GetHeight(fibitmap);
            using var bitmap = FreeImageAPI.FreeImage.GetBitmap(fibitmap);
            FreeImageAPI.FreeImage.Unload(fibitmap);

            // 相同图片判断
            var md5 = ImageComparer.GetMD5Hash(reader);
            var samePictures = new List<Picture>();
            var samePicture = Context.Pictures.Where(p => p.Hash == md5).SingleOrDefault();
            if (samePicture != null)
                samePictures.Add(samePicture);
            samePictures.AddRange(Pictures.Where(p => p.Hash == md5).ToList());

            // 相似判断
            var phash = ImageComparer.GetImagePHash(bitmap);
            var similarPictures = Context.Pictures.Where(p => p.WeakHash == phash).ToList();
            similarPictures.AddRange(Pictures.Where(p => p.WeakHash == phash).ToList());

            // 保存到临时文件夹
            string saveFileName;
            do
            {
                saveFileName = $"{md5}{Random.Shared.Next()}";
            } while (File.Exists(Path.Join(UserSettingData.TempFolderPath, saveFileName + Path.GetExtension(file))));
            File.Copy(file, Path.Join(UserSettingData.TempFolderPath, saveFileName + Path.GetExtension(file)));

            string? thumbFileName = null;
            // 生成缩略图
            if (width > 2560 || height > 2560)
            {
                thumbFileName = saveFileName + "_s.png";
                int targetWidth, targetHeight;
                if (width > 2560)
                {
                    targetWidth = 2560;
                    targetHeight = height * targetWidth / width;
                }
                else
                {
                    targetHeight = 2560;
                    targetWidth = width * targetHeight / height;
                }
                using var thumb = ImageUtility.Resize(bitmap, targetWidth, targetHeight);
                thumb.Save(Path.Join(UserSettingData.TempFolderPath, thumbFileName));
            }

            // 生成Picture数据
            var picture = new Picture
            {
                Title = Path.GetFileNameWithoutExtension(file),
                Path = saveFileName,
                ThumbnailPath = thumbFileName,
                Width = width,
                Height = height,
                Type = PictureType.LocalPicture,
                Hash = md5,
                WeakHash = phash,
                SamePicture = samePictures,
                SimilarPictures = similarPictures,
            };
        }
    }
}
