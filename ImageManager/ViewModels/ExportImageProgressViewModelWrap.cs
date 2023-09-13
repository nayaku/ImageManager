using HandyControl.Controls;
using HandyControl.Data;
using ImageManager.Data;
using ImageManager.Data.Model;
using ImageManager.Logging;
using StyletIoC;

namespace ImageManager.ViewModels
{
    public class ExportImageProgressViewModelWrap : ProgressViewModelWrapBase
    {
        [Inject]
        public ImageContext Context { get; set; }
        [Inject]
        public IContainer Container { get; set; }
        [Inject]
        public UserSettingData UserSettingData { get; set; }

        public List<Picture> Pictures { get; private set; }
        private string _dataFilePath;
        private Logger _logger = LoggerFactory.GetLogger(nameof(ExportImageProgressViewModelWrap));


        public ExportImageProgressViewModelWrap(List<Picture> pictures, string exportFilePath)
        {
            Pictures = pictures;
            _dataFilePath = exportFilePath;
        }

        protected override bool Process()
        {
            try
            {
                var pictureDataArchive = new PictureDataArchive(UserSettingData, Context, _dataFilePath)
                {
                    Pictures = Pictures,
                    ProgressChanged = (sender, e) =>
                    {
                        Progress = e.Progress;
                        Message = e.Message;
                    }
                };
                pictureDataArchive.Save(_cancellationToken);
                return true;
            }
            catch (Exception e)
            {
                DialogViewModel.Show(Container, "导出错误", e.Message, "确定");
                _logger.Error(e);
            }
            return false;
        }

        protected override void Cancel()
        {
            try
            {
                if (File.Exists(_dataFilePath))
                {
                    File.Delete(_dataFilePath);
                }
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }

        }

        protected override void Done()
        {
            var growlInfo = new GrowlInfo()
            {
                ConfirmStr = "打开输出目录",
                CancelStr = "取消",
                Message = $"导出成功，是否立刻打开输出目录？",
                ActionBeforeClose = isConfirmed =>
                {
                    if (isConfirmed)
                    {
                        // 打开输出目录
                        System.Diagnostics.Process.Start("explorer.exe", "/select," + _dataFilePath);
                    }
                    return true;

                },
                Token = "RootViewMessage"
            };
            Growl.Ask(growlInfo);

        }

        public enum ModeEnum
        {
            Export,
            Import
        }
    }
}
