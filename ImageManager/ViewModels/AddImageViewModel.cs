using HandyControl.Controls;
using ImageManager.Data;
using ImageManager.Data.Model;
using ImageManager.Views;
using Microsoft.EntityFrameworkCore;
using Stylet;
using StyletIoC;
using System.IO;

namespace ImageManager.ViewModels
{
    public class AddImageViewModel : Screen, IInjectionAware
    {
        [Inject]
        public IContainer Container { get; set; }
        [Inject]
        public ImageContext Context { get; set; }
        public MainPageViewModel MainPageViewModel { get; set; }
        public List<Picture> Pictures { get; set; }
        public bool WaitingAdding { get; set; }
        public bool Result { get; private set; }

        private bool _canClose = false;
        private bool _isWorking = false;
        private List<string> _files;
        private EventHandler<int> _successEvent;

        public AddImageViewModel(List<string> files, List<Picture> pictures, EventHandler<int> successEvent)
        {
            _files = files;
            Pictures = pictures;
            _successEvent = successEvent;

            MainPageViewModel = new(this)
            {
                UserSetting = new() { AutoSave = false },
                OrderBy = UserSettingData.OrderByEnum.AddState,
                IsGroup = true,
            };
        }

        public async void ShowMessageSync()
        {
            await Task.Delay(500);
            var samePictureNum = Pictures.Count(p => p.SamePicture != null);
            var similarPictureNum = Pictures.Count(p => p.SimilarPictures != null);
            var message = $"一共扫描到{Pictures.Count}张图片，其中有{samePictureNum}张重复图片，" +
                $"有{similarPictureNum}张相似图片，此外还有{_files.Count - Pictures.Count}个文件不是图片。";
            Growl.Success(message,"AddImageViewMessage");
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
            MainPageViewModel.AcceptToAdd(acceptString);
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
                        Context.Add(p);
                        count++;
                    }
                });
                Context.SaveChanges();
                return count;
            });
            dialog.Close();
            _canClose = true;
            Result = true;
            RequestClose();
            _successEvent?.Invoke(this, count);
        }
        public async void CancelAsync()
        {
            if (_isWorking)
            {
                return;
            }
            _canClose = true;
            Result = false;
            RequestClose();
        }

        public override Task<bool> CanCloseAsync()
        {
            return Task.Run(() =>
            {
                return _canClose;
            });
        }

        public void ParametersInjected()
        {
            Context.Database.Migrate();
            Container.BuildUp(MainPageViewModel);
            // 得手动调用
            MainPageViewModel.ParametersInjected();
        }
    }
}
