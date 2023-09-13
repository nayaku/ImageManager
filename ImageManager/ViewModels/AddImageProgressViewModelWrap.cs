using ImageManager.Data;
using ImageManager.Data.Model;
using ImageManager.Logging;
using ImageManager.Tools.Extension;
using StyletIoC;
using Logger = ImageManager.Logging.Logger;

namespace ImageManager.ViewModels
{
    public class AddImageProgressViewModelWrap : ProgressViewModelWrapBase, IInjectionAware
    {
        [Inject]
        public UserSettingData UserSettingData { get; set; }
        [Inject]
        public ImageContext Context { get; set; }
        [Inject]
        public IWindowManager WindowManager { get; set; }
        [Inject]
        public IContainer Container { get; set; }

        private List<string> _pictureFiles = new();
        private int _notPictureNum = 0;
        private int _archiveNum = 0;
        private List<Picture> _pictures = new();
        private List<string> _dirFiles;
        private Logger _logger = LoggerFactory.GetLogger(nameof(AddImageProgressViewModelWrap));
        private EventHandler<int> _successEvent;

        /// <summary>
        /// 数据库在第二部分进度条中的权重
        /// </summary>
        private readonly double _archiveProgressWeight = 20;
        /// <summary>
        /// 用于进度条第二部分计算的总和
        /// </summary>
        private double _progressSum => (_pictureFiles.Count - _archiveNum) + _archiveNum * _archiveProgressWeight;


        public AddImageProgressViewModelWrap(List<string> dirFiles, EventHandler<int> successEvent)
        {
            _dirFiles = dirFiles;
            _successEvent = successEvent;
        }

        /// <summary>
        /// 处理
        /// </summary>
        /// <returns>处理成功</returns>
        protected override bool Process()
        {
            // 先遍历一遍，把文件都找出来
            // 然后遍历文件，找出受支持格式的图片

            Message = "正在准备处理...";
            // 找出文件
            foreach (var dirFile in _dirFiles)
            {
                if (_cancellationToken.IsCancellationRequested)
                    return false;

                if (Directory.Exists(dirFile))
                {
                    // 如果为文件夹，遍历文件夹
                    Message = $"正在查找{dirFile}下的文件...";
                    _pictureFiles.AddRange(Directory.GetFiles(dirFile, "*.*", SearchOption.AllDirectories));
                }
                else
                {
                    // 如果为文件，直接添加
                    _pictureFiles.Add(dirFile);
                    if (dirFile.EndsWith(PictureDataArchive.Extension))
                        _archiveNum++;
                }
                Progress += 1.0 / _dirFiles.Count * 10;
            }
            Progress = 10;

            var progressCount = 0.0;
            var pictureFactory = new PictureFactory(UserSettingData, Context);
            // 遍历文件
            foreach (var file in _pictureFiles)
            {
                if (_cancellationToken.IsCancellationRequested)
                    return false;
                Message = $"正在处理{file}...";

                try
                {
                    if (PictureDataArchive.IsPictureDataArchiveFile(file))
                    {
                        var pictureDataArchive = new PictureDataArchive(UserSettingData, Context, file)
                        {
                            ProgressChanged = (sender, e) =>
                            {
                                Message = $"正在导入图片数据库{Path.GetFileName(file)}:" +
                                $"{e.Message}({e.Progress}%)";
                                Progress = (e.Progress * (_archiveProgressWeight / _progressSum)
                                + 100.0 * progressCount / _progressSum) * 0.9 + 0.1;
                            }
                        };
                        pictureDataArchive.Load(_cancellationToken, _pictures.AsReadOnly());
                        _pictures.AddRange(pictureDataArchive.Pictures);
                        progressCount += _archiveProgressWeight;
                    }
                    else if (PictureFactory.IsPictureFile(file))
                    {
                        var picture = pictureFactory.CreateTempPicture(file, _pictures.AsReadOnly(), false);
                        _pictures.Add(picture);
                        progressCount += 1;
                    }
                    else
                    {
                        _notPictureNum++;
                    }
                }
                catch (Exception e)
                {
                    DialogViewModel.Show(Container, "导入错误", file + ":" + e.Message, "确定");
                    _logger.Error(e);
                }
                Progress = progressCount / _progressSum * 90 + 10;

            }
            if (_cancellationToken.IsCancellationRequested)
                return false;
            return true;
        }

        /// <summary>
        /// 做取消清理工作
        /// </summary>
        protected override void Cancel()
        {
            var preProgress = Progress;
            foreach (var picture in _pictures)
            {
                Message = $"删除文件{picture.Path}";
                picture.SafeDeleteFile();
                Progress -= 1.0 / _pictures.Count * preProgress * 90;
            }
        }

        protected override void Done()
        {
            // 准备完成，等待用户选择需要添加的图片
            var samePictureNum = _pictures.Count(p => p.SamePicture?.Any() ?? false);
            var similarPictureNum = _pictures.Count(p => p.SimilarPictures?.Any() ?? false);
            var message = $"一共扫描到{_pictures.Count}张图片，其中有{_archiveNum}个图片数据库，" +
                $"有{samePictureNum}张重复图片，有{similarPictureNum}张相似图片，此外还有{_notPictureNum}个文件不支持。";

            var addImageViewModel = new AddImageViewModel(_pictures, message, _successEvent, Context);
            Container.BuildUpEx(addImageViewModel);

            Execute.OnUIThread(() =>
            {
                WindowManager.ShowWindow(addImageViewModel);
            });
        }
    }
}
