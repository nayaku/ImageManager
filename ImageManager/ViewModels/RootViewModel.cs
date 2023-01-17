using HandyControl.Controls;
using HandyControl.Themes;
using ImageManager.Data;
using ImageManager.Data.Model;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;
using Stylet;
using StyletIoC;
using System.Collections.Generic;
using System.Diagnostics;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Label = ImageManager.Data.Model.Label;

namespace ImageManager.ViewModels
{
    public class RootViewModel : PropertyChangedBase, IInjectionAware
    {
        [Inject]
        public UserSettingData UserSettingData { get; set; }
        [Inject]
        public ImageContext Context { get; set; }
        public bool ThemeConfigShow { get; set; } = false;
        public string SearchText { get; set; }
        public List<Picture> Pictures { get; set; }
        public List<Label> SearchedLabels { get; set; }
        public MainPageViewModel MainPageViewModel { get; set; }
        public bool CanAddImage { get; set; } = true;
        public bool ShowLabelPopup { get; set; }

        private IWindowManager _windowManager;
        public RootViewModel(IWindowManager windowManager, IContainer container)
        {
            _windowManager = windowManager;
            MainPageViewModel = new(this, windowManager, container);
            container.BuildUp(MainPageViewModel);
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
            // 消除焦点
            Keyboard.ClearFocus();
            FocusManager.SetFocusedElement((DependencyObject)sender, (IInputElement)sender);
        }

        #region 菜单栏
        public void AddPictures()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                //dialog.Filter = "图片文件|*.BMP;*ICO;*.JPG;*.JIF;*.JPEG;*.JPE;*.JNG;*.KOA;*.IFF;*.LBM;*.IFF;*.LBM;*.MNG;*.PBM;*.PCD;*PCX;*.PGM;*.PNG;*.PPM;*.PPM;*.RAS;*.TGA;*.TARGA;*.TIF;*.TIFF;*.WBMP;*.PSD;*.CUT;*.XBM;*.XPM;*.DDS;*.GIF;*.HDR;*.G3;*.SGI;*.EXR;*.J2K;*.J2C;*.JP2;*.PFM;*.PICT;*.RAW;*.webp;*.jxr";
                Multiselect = true
            };
            bool? fileDialogResult = dialog.ShowDialog();
            if (fileDialogResult ?? false)
            {
                // 扫描和准备文件
                var progressViewModel = new ProgressViewModel(new List<string>(dialog.FileNames));
                var progressDialogResult = _windowManager.ShowDialog(progressViewModel);
                if (progressDialogResult ?? false)
                {
                    // 添加图片
                    var addImageViewModel = new AddImageViewModel(progressViewModel.Files, progressViewModel.Pictures);
                    var addImageDialogResult = _windowManager.ShowDialog(addImageViewModel);
                    // TODO：处理添加成功
                }

            }
        }

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
        public void CopyPicture()
        {
            MainPageViewModel.CopyPicture();
        }
        public void DeletePicture()
        {
            MainPageViewModel.DeletePicture();
        }
        public void ScreenShot()
        {
            throw new NotImplementedException();
        }
        #endregion

        public void ParametersInjected()
        {
            Context.Database.Migrate();
            // 得手动调用
            MainPageViewModel.ParametersInjected();
            ThemeManager.Current.ApplicationTheme = UserSettingData.Theme;
        }
    }
}
