using HandyControl.Controls;
using HandyControl.Data;
using HandyControl.Themes;
using ImageManager.Data;
using ImageManager.Tools;
using ImageManager.Windows;
using StyletIoC;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Label = ImageManager.Data.Model.Label;

namespace ImageManager.ViewModels
{
    public class RootViewModel : Screen, IInjectionAware
    {
        [Inject]
        public UserSettingData UserSettingData { get; set; }
        [Inject]
        public ImageContext Context { get; set; }
        public bool ThemeConfigShow { get; set; } = false;
        public string SearchText { get; set; }
        public List<Label> SearchedLabels { get; set; }
        public MainPageViewModel MainPageViewModel { get; set; }
        public bool ShowLabelPopup { get; set; }
        public WindowState WindowState { get; set; }
        public bool IsHideWhenScreenShoot
        {
            get => UserSettingData.IsHideWhenScreenShoot;
            set => UserSettingData.IsHideWhenScreenShoot = value;
        }

        private IWindowManager _windowManager;
        private IContainer _container;
        public RootViewModel(IWindowManager windowManager, IContainer container)
        {
            _windowManager = windowManager;
            _container = container;
            MainPageViewModel = new(this);
            container.BuildUp(MainPageViewModel);
        }

        public void Loaded()
        {
            var res = !HotKey.Regist((System.Windows.Window)View, HotKey.KeyModifiers.Ctrl | HotKey.KeyModifiers.Shift | HotKey.KeyModifiers.Alt, Key.X, () =>
            {
                ScreenShot();
            });
            // 注册失败
            if (!res)
            {
                Growl.Error("注册截图热键失败");
            }

            // 检查更新
            CheckUpdateAsync();
        }

        public void UpdateSearchedLabels()
        {
            if (SearchText == null || SearchText == "")
                SearchedLabels = Context.Labels.OrderByDescending(l => l.Num).ToList();
            else
                SearchedLabels = Context.Labels.Where(l => l.Name.Contains(SearchText)).OrderByDescending(l => l.Num).ToList();
        }
        public void LabelClick(Label label)
        {
            if (!MainPageViewModel.FilterLabels.Contains(label))
                MainPageViewModel.FilterLabels.Add(label);
        }
        public void SearchBarGotFocus()
        {
            UpdateSearchedLabels();
            Task.Run(async () =>
            {
                await Task.Delay(100);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ShowLabelPopup = true;
                });
            });
        }
        public void SearchBarLostFocus()
        {
            Task.Run(async () =>
            {
                await Task.Delay(100);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ShowLabelPopup = false;
                });
            });
        }
        public void SearchStarted(string searchText)
        {
            MainPageViewModel.UpdatePicture();
        }
        public void ShowThemeConfig() => ThemeConfigShow = true;
        public void ChangeTheme(ApplicationTheme theme)
        {
            if (ThemeManager.Current.ApplicationTheme != theme)
            {
                ThemeManager.Current.ApplicationTheme = theme;
                UserSettingData.Theme = theme;
                UserSettingData.Save();
            }
        }
        public void WindowMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //// 消除焦点
            //Keyboard.ClearFocus();
            //FocusManager.SetFocusedElement((DependencyObject)sender, (IInputElement)sender);
        }

        #region 菜单栏
        private Dispatcher _dispatcher;
        public void AddPictures()
        {
            _dispatcher = Application.Current.Dispatcher;
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                //dialog.Filter = "图片文件|*.BMP;*ICO;*.JPG;*.JIF;*.JPEG;*.JPE;*.JNG;*.KOA;*.IFF;*.LBM;*.IFF;*.LBM;*.MNG;*.PBM;*.PCD;*PCX;*.PGM;*.PNG;*.PPM;*.PPM;*.RAS;*.TGA;*.TARGA;*.TIF;*.TIFF;*.WBMP;*.PSD;*.CUT;*.XBM;*.XPM;*.DDS;*.GIF;*.HDR;*.G3;*.SGI;*.EXR;*.J2K;*.J2C;*.JP2;*.PFM;*.PICT;*.RAW;*.webp;*.jxr";
                Multiselect = true
            };
            bool? fileDialogResult = dialog.ShowDialog();
            if (fileDialogResult ?? false)
            {
                // 扫描和准备文件
                var progressViewModel = new AddImageProgressViewModel(new List<string>(dialog.FileNames), AddPictureSuccessEvent);
                _container.BuildUp(progressViewModel);
                progressViewModel.ParametersInjected();
                _windowManager.ShowWindow(progressViewModel);
            }
        }
        public void AddPictureSuccessEvent(object? sender, int pictureNum)
        {
            var growlInfo = new GrowlInfo()
            {
                ConfirmStr = "更新",
                CancelStr = "取消",
                Message = $"成功添加{pictureNum}张图片，是否确定立刻更新主界面？",
                ActionBeforeClose = isConfirmed =>
                {
                    if (isConfirmed)
                    {
                        MainPageViewModel.UpdatePicture();
                        Growl.Info("已更新主界面", "RootViewMessage");
                    }
                    return true;

                },
                Token = "RootViewMessage"
            };
            Growl.Ask(growlInfo);
        }
        public void ScreenShot()
        {
            if(IsHideWhenScreenShoot)
            {
                var preWindowState = WindowState;
                WindowState = WindowState.Minimized;
                Execute.PostToUIThreadAsync(async () =>
                {
                    await Task.Delay(300);
                    var screenShotWindow = new ScreenShotWindow();
                    screenShotWindow.Show();
                    await Task.Delay(1500);
                    WindowState = preWindowState;
                });
            }
            else
            {
                var screenShotWindow = new ScreenShotWindow();
                screenShotWindow.Show();
            }
            
        }
        public void CheckUpdate()
        {
            var updateViewModel = new UpdateViewModel();
            _windowManager.ShowWindow(updateViewModel);
        }
        public void About()
        {
            var aboutViewModel = new AboutViewModel();
            _windowManager.ShowWindow(aboutViewModel);
        }
        #endregion

        private async void CheckUpdateAsync()
        {
            //await Task.Delay(1000);
            var updateViewModel = new UpdateViewModel();
            if (await updateViewModel.NeedUpdateAsync())
            {
                var growlInfo = new GrowlInfo()
                {
                    ConfirmStr = "更新",
                    CancelStr = "取消",
                    Message = $"发现新版本{updateViewModel.LatestVersion}，是否更新？",
                    ActionBeforeClose = isConfirmed =>
                    {
                        if (isConfirmed)
                            _windowManager.ShowWindow(updateViewModel);
                        return true;

                    },
                    Token = "RootViewMessage"
                };
                Growl.Ask(growlInfo);
            }
        }

        public void ParametersInjected()
        {
            // 得手动调用
            MainPageViewModel.ParametersInjected();
            ThemeManager.Current.ApplicationTheme = UserSettingData.Theme;
        }
        public void Closed()
        {
            App.Current.Shutdown();
        }
    }
}
