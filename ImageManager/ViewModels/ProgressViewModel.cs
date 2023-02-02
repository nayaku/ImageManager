using ImageManager.Data;
using ImageManager.Data.Model;
using ImageManager.Tools;
using Stylet;
using StyletIoC;
using System.IO;

namespace ImageManager.ViewModels
{
    public class ProgressViewModel : Screen, IInjectionAware
    {
        [Inject]
        public UserSettingData UserSettingData { get; set; }
        [Inject]
        public IWindowManager WindowManager { get; set; }
        [Inject]
        public IContainer Container { get; set; }
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
        private EventHandler<int> _successEvent;

        public ProgressViewModel(List<string> dirFiles, EventHandler<int> successEvent)
        {
            _dirFiles = dirFiles;
            _successEvent = successEvent;
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
                Message = $"删除文件{picture.Path}";
                File.Delete(Path.Join(picture.ImageFolderPath, picture.Path));
                if (picture.ThumbnailPath != null)
                    File.Delete(Path.Join(picture.ImageFolderPath, picture.ThumbnailPath));
                Progress -= 1.0 / Pictures.Count * preProgress * 100;
            }
        }
        public async void DoTask()
        {
            // 先遍历一遍，把文件都找出来
            // 然后遍历文件，找出受支持格式的图片
            _canClose = false;
            var result = await Task.Run(() =>
            {
                Message = "正在准备处理...";
                // 找出文件
                foreach (var dirFile in _dirFiles)
                {
                    if (_cancellationTokenSource.Token.IsCancellationRequested)
                        return false;
                    Walk(dirFile);
                }
                Progress = 20;

                // 遍历文件
                foreach (var file in Files)
                {
                    if (_cancellationTokenSource.Token.IsCancellationRequested)
                        return false;
                    Message = $"正在处理{file}...";
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
            RequestClose();
            if (result)
            {
                // 准备完成，等待用户选择需要添加的图片
                var addImageViewModel = new AddImageViewModel(Files, Pictures, _successEvent);
                Container.BuildUp(addImageViewModel);
                addImageViewModel.ParametersInjected();
                WindowManager.ShowWindow(addImageViewModel);
            }
        }

        private void Walk(string path)
        {
            if (_cancellationTokenSource.Token.IsCancellationRequested)
                return;
            // 文件
            if (File.Exists(path))
                Files.Add(path);
            else
            {
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
        }

        private void ProcessFile(string file)
        {
            var FileStream = File.OpenRead(file);
            var reader = new BufferedStream(FileStream, 16 * 1024 * 1024); //16MB

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
            reader.Seek(0, SeekOrigin.Begin);
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
                accept = similarPictures.All(p => width * height > p.Width * p.Height);
            else
                accept = true;


            // 生成Picture数据
            var dirPath = UserSettingData.Default.TempFolderPath;
            if (!Path.IsPathRooted(dirPath))
                dirPath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, dirPath);
            var picture = new Picture(false)
            {
                Title = Path.GetFileNameWithoutExtension(file),
                Path = saveFileName + Path.GetExtension(file),
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
            Pictures.Add(picture);
        }

        public void ParametersInjected()
        {

        }
    }
}
