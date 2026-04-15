using HandyControl.Controls;
using ImageManager.Data;
using ImageManager.Data.Model;
using ImageManager.Logging;
using ImageManager.Views;
using StyletIoC;
using System.Windows;

namespace ImageManager.ViewModels
{
    public class AddImageViewModel : Screen, IInjectionAware
    {
        [Inject]
        public IContainer Container { get; set; }

        public MainPageViewModel MainPageViewModel { get; set; }
        public List<Picture> Pictures { get; set; }
        public bool WaitingAdding { get; set; }
        public bool Result { get; private set; }

        private bool _canClose = false;
        private bool _isWorking = false;
        private string _message;
        private EventHandler<int> _successEvent;
        private ImageContext _context;
        private Logger _logger = LoggerFactory.GetLogger(nameof(AddImageViewModel));


        public AddImageViewModel(List<Picture> pictures, string message, EventHandler<int> successEvent, ImageContext context)
        {
            Pictures = pictures;
            _message = message;
            _successEvent = successEvent;

            MainPageViewModel = new(this, context)
            {
                UserSetting = new() { AutoSave = false },
                OrderBy = UserSettingData.OrderByEnum.AddState,
                IsGroup = true,
            };
            _context = context;
        }

        public async void ShowMessageSync()
        {
            await Task.Delay(500);
            Growl.Success(_message, "AddImageViewMessage");
        }

        #region 快捷键
        public void SelectAll()
        {
            MainPageViewModel.SelectAll();
        }
        public void SelectNone()
        {
            MainPageViewModel.SelectNone();
        }
        public void SelectInvert()
        {
            MainPageViewModel.SelectInvert();
        }
        public void AcceptToAdd(string acceptString)
        {
            MainPageViewModel.AcceptToAdd(bool.Parse(acceptString));
        }
        #endregion

        public async Task AcceptAsync()
        {
            _isWorking = true;
            var dialog = Dialog.Show(new WaitingDialog());
            try
            {
                var toAddPictures = Pictures.Where(p => p.AcceptToAdd).ToList();
                toAddPictures.ForEach(p =>
                {
                    // 由于文件被占用删不掉，所以只能复制到新的位置，之后再删除原文件
                    var tempDir = p.ImageFolderPath;
                    p.SetDefaultImageFolderPath();
                    var file = Path.Combine(tempDir, p.Path);
                    var newFile = Path.Combine(p.ImageFolderPath, p.Path);
                    File.Copy(file, newFile);
                    if (p.ThumbnailPath != null)
                    {
                        var thumbnailFile = Path.Combine(tempDir, p.ThumbnailPath);
                        var newThumbnailFile = Path.Combine(p.ImageFolderPath, p.ThumbnailPath);
                        File.Copy(thumbnailFile, newThumbnailFile);
                    }
                });
                _context.Pictures.AddRange(toAddPictures);
                await _context.SaveChangesAsync();
                _successEvent?.Invoke(this, toAddPictures.Count);
            }
            catch (Exception ex)
            {
                DialogViewModel.Show(Container, "添加失败", ex.Message, "确定");
                _logger.Error(ex);
                return;
            }
            finally
            {
                dialog.Close();   // 无论成功还是失败都关闭
                _isWorking = false;
                _canClose = true;
                Result = true;
                RequestClose();
            }
        }
        public void PreClosing()
        {
            if (_isWorking)
            {
                return;
            }
            _canClose = true;
            Result = false;
        }
        public void Cancel()
        {
            PreClosing();
            RequestClose();
        }

        public override Task<bool> CanCloseAsync()
        {
            return Task.FromResult(_canClose);
        }

        public void ParametersInjected()
        {
            Container.BuildUp(MainPageViewModel);
            MainPageViewModel.ParametersInjected();
        }
    }
}
