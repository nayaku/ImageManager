using HandyControl.Controls;
using HandyControl.Data;
using HandyControl.Themes;
using ImageManager.Data;
using ImageManager.Tools;
using ImageManager.Tools.Extension;
using ImageManager.Windows;
using StyletIoC;
using System.Windows;
using System.Windows.Input;
using IContainer = StyletIoC.IContainer;
using Label = ImageManager.Data.Model.Label;

namespace ImageManager.ViewModels
{
    public class RootViewModel : Screen, IInjectionAware
    {
        private ImageContext _context;

        [Inject]
        public UserSettingData UserSettingData { get; set; }
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
        public RootViewModel(IWindowManager windowManager, IContainer container, ImageContext context)
        {
            _windowManager = windowManager;
            _container = container;
            MainPageViewModel = new(this, context);
            _context = context;
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
                SearchedLabels = _context.Labels.OrderByDescending(l => l.Num).ToList();
            else
                SearchedLabels = _context.Labels.Where(l => l.Name.Contains(SearchText)).OrderByDescending(l => l.Num).ToList();
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
            MainPageViewModel.RefreshPicture();
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
        /// <summary>
        /// 添加图片
        /// </summary>
        public void AddPictures()
        {
            var dialog = new System.Windows.Forms.OpenFileDialog
            {
                Multiselect = true
            };
            var res = dialog.ShowDialog();
            if (res == System.Windows.Forms.DialogResult.OK)
            {
                // 扫描和准备文件
                var addImageProgressViewModelWrap = new AddImageProgressViewModelWrap(
                    new List<string>(dialog.FileNames), AddPictureSuccess);
                _container.BuildUpEx(addImageProgressViewModelWrap);
                _windowManager.ShowWindow(addImageProgressViewModelWrap.ProgressViewModel);
            }
        }

        /// <summary>
        /// 添加文件夹
        /// </summary>
        public void AddFolders()
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            var result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                // 扫描和准备文件
                var addImageProgressViewModelWrap = new AddImageProgressViewModelWrap(
                                       new List<string>() { dialog.SelectedPath }, AddPictureSuccess);
                _container.BuildUpEx(addImageProgressViewModelWrap);
                _windowManager.ShowWindow(addImageProgressViewModelWrap.ProgressViewModel);
            }
        }

        /// <summary>
        /// 导入数据库
        /// </summary>
        public void ImportData()
        {
            // 选择要导入的文件
            var dialog = new System.Windows.Forms.OpenFileDialog
            {
                Filter = "数据库文件|*" + PictureDataArchive.Extension,
                Multiselect = true,
            };
            var result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                var addImageProgressViewModel = new AddImageProgressViewModelWrap(
                                       new List<string>(dialog.FileNames), AddPictureSuccess);
                _container.BuildUpEx(addImageProgressViewModel);
                _windowManager.ShowWindow(addImageProgressViewModel.ProgressViewModel);
            }
        }

        private void AddPictureSuccess(object? sender, int pictureNum)
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
                        MainPageViewModel.RefreshPicture();
                        Growl.Info("已更新主界面", "RootViewMessage");
                    }
                    return true;

                },
                Token = "RootViewMessage"
            };
            Growl.Ask(growlInfo);
        }

        /// <summary>
        /// 导出数据库
        /// </summary>
        public void ExportData()
        {
            // 选择要导出到的文件夹和文件名
            var dialog = new System.Windows.Forms.SaveFileDialog
            {
                Filter = "数据库文件|*" + PictureDataArchive.Extension,
            };
            var result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                var exportImageProgressViewModelWrap = new ExportImageProgressViewModelWrap(
                    _context.Pictures.ToList(), dialog.FileName);
                _container.BuildUpEx(exportImageProgressViewModelWrap);
                _windowManager.ShowWindow(exportImageProgressViewModelWrap.ProgressViewModel);
            }
        }

        /// <summary>
        /// 截图
        /// </summary>
        public void ScreenShot()
        {
            if (IsHideWhenScreenShoot)
            {
                var preWindowState = WindowState;
                if (preWindowState != WindowState.Minimized)
                {
                    WindowState = WindowState.Minimized;
                    Execute.PostToUIThreadAsync(async () =>
                    {
                        await Task.Delay(500);
                        ScreenShotWindow.ShowScreenShotWindow();
                        await Task.Delay(300);
                        WindowState = preWindowState;
                    });
                }
                else
                {
                    ScreenShotWindow.ShowScreenShotWindow();
                }
            }
            else
            {
                ScreenShotWindow.ShowScreenShotWindow();
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
            _container.BuildUp(MainPageViewModel);
            MainPageViewModel.ParametersInjected();
            ThemeManager.Current.ApplicationTheme = UserSettingData.Theme;
        }

        public void Closed()
        {
            Application.Current.Shutdown();
        }
    }
}
