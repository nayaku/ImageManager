using HandyControl.Controls;
using ImageManager.Data;
using ImageManager.Data.Model;
using ImageManager.Views;
using StyletIoC;

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
            var count = await Task.Run(() =>
            {
                var count = 0;
                Pictures.ForEach(p =>
                {
                    // 由于文件被占用删不掉
                    if (p.AcceptToAdd)
                    {
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
                        _context.Add(p);
                        count++;
                    }
                });
                _context.SaveChanges();
                return count;
            });
            dialog.Close();
            _canClose = true;
            Result = true;
            RequestClose();
            _successEvent?.Invoke(this, count);
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
