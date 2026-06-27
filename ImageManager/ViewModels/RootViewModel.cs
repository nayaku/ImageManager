using HandyControl.Controls;
using HandyControl.Data;
using HandyControl.Themes;
using ImageManager.Data;
using ImageManager.Logging;
using ImageManager.Tools;
using ImageManager.Tools.Extension;
using ImageManager.Windows;
using StyletIoC;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Application = System.Windows.Application;
using Clipboard = System.Windows.Clipboard;
using DataFormats = System.Windows.DataFormats;
using DragDropEffects = System.Windows.DragDropEffects;
using DragEventArgs = System.Windows.DragEventArgs;
using IContainer = StyletIoC.IContainer;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using Label = ImageManager.Data.Model.Label;
using Screen = Stylet.Screen;
using Window = System.Windows.Window;

namespace ImageManager.ViewModels
{
    public class RootViewModel : Screen, IInjectionAware
    {
        private ImageContext _context;
        private IWindowManager _windowManager;
        private IContainer _container;

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
        public bool RestoreStickerOnStartup
        {
            get => UserSettingData.RestoreStickerOnStartup;
            set => UserSettingData.RestoreStickerOnStartup = value;
        }
        public bool IsClipboardContainsImage => Clipboard.ContainsImage();

        public RootViewModel(IWindowManager windowManager, IContainer container, ImageContext context)
        {
            _windowManager = windowManager;
            _container = container;
            MainPageViewModel = new(this, context);
            _context = context;
        }

        public void Loaded()
        {
            var res = !HotKey.Regist((Window)View, HotKey.KeyModifiers.Ctrl | HotKey.KeyModifiers.Shift | HotKey.KeyModifiers.Alt, Key.X, () =>
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

            // 还原或清理贴片
            RestoreOrClearStickers();
        }

        /// <summary>
        /// 启动时：勾选则还原上次打开的贴片并清理孤儿图片；未勾选则清空全部贴片配置与 STMP 图片。
        /// </summary>
        private void RestoreOrClearStickers()
        {
            var folder = UserSettingData.StickerFolderPath;
            if (!Directory.Exists(folder))
                return;

            if (UserSettingData.RestoreStickerOnStartup)
            {
                var validFiles = new HashSet<string>();
                foreach (var fileName in UserSettingData.Stickers.ToList())
                {
                    var imagePath = Path.Join(folder, fileName);
                    var statePath = Path.Join(folder, fileName + ".xml");
                    var state = SettingsBase.Load<StickerStateData>(statePath);
                    if (state != null && File.Exists(imagePath))
                    {
                        validFiles.Add(fileName);
                        validFiles.Add(fileName + ".xml");
                        _windowManager.ShowWindow(new StickerViewModel(state));
                    }
                    else
                    {
                        // 图片或状态缺失，剔除残留登记
                        UserSettingData.Stickers.Remove(fileName);
                    }
                }
                // 删除未被任何贴片引用的孤儿文件（图片与 xml）
                foreach (var file in Directory.GetFiles(folder))
                {
                    if (!validFiles.Contains(Path.GetFileName(file)))
                    {
                        try
                        {
                            File.Delete(file);
                        }
                        catch (Exception ex)
                        {
                            LoggerFactory.GetLogger(nameof(RootViewModel)).Error(ex);
                        }
                    }
                }
            }
            else
            {
                // 不还原：清空登记与全部 STMP 文件
                UserSettingData.Stickers.Clear();
                foreach (var file in Directory.GetFiles(folder))
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch (Exception ex)
                    {
                        LoggerFactory.GetLogger(nameof(RootViewModel)).Error(ex);
                    }
                }
            }
        }

        public void UpdateSearchedLabels()
        {
            var query = _context.Labels.AsQueryable();
            var searchText = SearchText?.Trim();
            if (!string.IsNullOrEmpty(searchText))
                query = query.Where(l => l.Name.Contains(searchText));
            SearchedLabels = query.OrderByDescending(l => l.Num).ToList();
        }
        public void LabelClick(Label label)
        {
            if (!MainPageViewModel.FilterLabels.Contains(label))
                MainPageViewModel.FilterLabels.Add(label);
            SearchText = string.Empty;
        }
        public void SearchBarGotFocus()
        {
            UpdateSearchedLabels();
            ShowLabelPopup = true;
        }
        public void SearchBarLostFocus()
        {
            ShowLabelPopup = false;
        }
        public void SearchBarKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                // 取消搜索
                SearchText = string.Empty;
                ClearSerchBarFocus();
            }
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
                UserSettingData.Flush();
            }
        }
        private void ClearSerchBarFocus()
        {
            Keyboard.ClearFocus();
            FocusManager.SetFocusedElement(View, View);
        }
        public void WindowMouseDown(object sender, MouseButtonEventArgs e)
        {
            // 消除焦点
            ClearSerchBarFocus();
        }

        #region 菜单栏
        public void PictureMenuSubmenuOpened()
        {
            NotifyOfPropertyChange(() => IsClipboardContainsImage);
        }

        /// <summary>
        /// 添加图片
        /// </summary>
        public void AddPictures()
        {
            var dialog = new OpenFileDialog
            {
                Multiselect = true
            };
            var res = dialog.ShowDialog();
            if (res == DialogResult.OK)
            {
                // 扫描和准备文件
                AddPicturesInner([.. dialog.FileNames]);
            }
        }

        public void DragOver(object sender, DragEventArgs e)
        {
            e.Effects = (e.Data.GetDataPresent(DataFormats.FileDrop) ||
                         e.Data.GetDataPresent(DataFormats.Bitmap) ||
                         e.Data.GetDataPresent("FileContents"))
                ? DragDropEffects.Copy
                : DragDropEffects.None;
            e.Handled = true;
        }

        public void Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) &&
                e.Data.GetData(DataFormats.FileDrop) is string[] paths &&
                paths.Length > 0)
            {
                AddPicturesInner([.. paths]);
            }
            else if (e.Data.GetDataPresent(DataFormats.Bitmap) &&
                     e.Data.GetData(DataFormats.Bitmap) is BitmapSource bitmap)
            {
                var tempFile = Path.GetTempFileName();
                using (var fileStream = new FileStream(tempFile, FileMode.Create))
                {
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bitmap));
                    encoder.Save(fileStream);
                }
                AddPicturesInner([tempFile]);
            }
            else if (e.Data.GetDataPresent("FileContents"))
            {
                var tempFile = VirtualFileHelper.SaveVirtualFileToDisk(e.Data);
                if (tempFile.Length > 0)
                    AddPicturesInner([.. tempFile]);
            }
        }

        public void AddPicturesInner(List<string> dirFiles)
        {
            var addImageProgressViewModelWrap = new AddImageProgressViewModelWrap(
                    dirFiles, AddPictureSuccess);
            _container.BuildUpEx(addImageProgressViewModelWrap);
            _windowManager.ShowWindow(addImageProgressViewModelWrap.ProgressViewModel);
        }

        /// <summary>
        /// 添加文件夹
        /// </summary>
        public void AddFolders()
        {
            var dialog = new FolderBrowserDialog();
            var result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                // 扫描和准备文件
                var addImageProgressViewModelWrap = new AddImageProgressViewModelWrap(
                                       new List<string>() { dialog.SelectedPath }, AddPictureSuccess);
                _container.BuildUpEx(addImageProgressViewModelWrap);
                _windowManager.ShowWindow(addImageProgressViewModelWrap.ProgressViewModel);
            }
        }

        public void AddClipboardImage()
        {
            if (Clipboard.ContainsImage())
            {
                var image = Clipboard.GetImage();
                if (image != null)
                {
                    // 保存到临时文件
                    var tempFile = Path.GetTempFileName();
                    using (var fileStream = new FileStream(tempFile, FileMode.Create))
                    {
                        var encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(image));
                        encoder.Save(fileStream);
                    }
                    AddPicturesInner([tempFile]);
                }
            }
        }

        public void StickClipboardImage()
        {
            if (Clipboard.ContainsImage())
            {
                var image = Clipboard.GetImage();
                if (image != null)
                {
                    var bitmap = ImageUtility.ImageSourceToBitmap(image);
                    var stickerViewModel = new StickerViewModel(bitmap);
                    _windowManager.ShowWindow(stickerViewModel);
                }
            }
        }

        /// <summary>
        /// 导入数据库
        /// </summary>
        public void ImportData()
        {
            // 选择要导入的文件
            var dialog = new OpenFileDialog
            {
                Filter = "数据库文件|*" + PictureDataArchive.Extension,
                Multiselect = true,
            };
            var result = dialog.ShowDialog();
            if (result == DialogResult.OK)
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
            var dialog = new SaveFileDialog
            {
                Filter = "数据库文件|*" + PictureDataArchive.Extension,
            };
            var result = dialog.ShowDialog();
            if (result == DialogResult.OK)
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
                        ScreenShotWindow.ShowScreenShotWindow(_windowManager);
                        await Task.Delay(300);
                        WindowState = preWindowState;
                    });
                }
                else
                {
                    ScreenShotWindow.ShowScreenShotWindow(_windowManager);
                }
            }
            else
            {
                ScreenShotWindow.ShowScreenShotWindow(_windowManager);
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

        protected override void OnClose()
        {
            // 标记退出：随后 Shutdown 逐个关闭贴片时，各贴片只落盘自身状态、不删文件
            StickerViewModel.IsShuttingDown = true;
            UserSettingData.Flush();
            Application.Current.Shutdown();
        }
    }
}
