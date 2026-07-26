using FreeImageAPI;
using ImageManager.Data;
using ImageManager.Tools;
using ImageManager.Views;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Path = System.IO.Path;
using Point = System.Windows.Point;
using Window = HandyControl.Controls.Window;

namespace ImageManager.ViewModels
{
    public class StickerViewModelWrapper(StickerViewModel sticker) : ICommand
    {
        public BitmapImage ThumbnailSource => sticker.ThumbnailSource;

        bool ICommand.CanExecute(object? parameter) => true;
        void ICommand.Execute(object? parameter) => sticker.FocusAndExpand();
        event EventHandler? ICommand.CanExecuteChanged { add { } remove { } }
    }

    public class StickerViewModel : Screen
    {
        private readonly Bitmap _sourceBitmap;
        private readonly BitmapImage _originalImageSource;
        private readonly StickerStateData _state;
        private readonly StickerViewModelWrapper _wrapper;
        private readonly Point? _initPoint;
        private readonly bool _isRestore;
        private readonly bool _canZoomInInitially; // 是否允许初始缩放（来自剪贴板和图库文件时允许）

        public static ObservableCollection<StickerViewModelWrapper> Instances { get; } = [];

        public ImageSource ImageSource { get; private set; }
        public BitmapImage ThumbnailSource { get; private set; }

        /// <summary>
        /// 贴片持久化状态
        /// </summary>
        public StickerStateData StickerState => _state;

        /// <summary>
        /// 来自截图/剪贴板时才可加入图库
        /// </summary>
        public bool CanAddToDatabase => !_state.IsFromDatabase;

        // 来自图库文件：复制原文件保留格式
        public StickerViewModel(string imagePath) : this(LoadBitmapFromFile(imagePath),
                  new StickerStateData(SaveImageFromFile(imagePath), true), null, false, true)
        { }
        // 来自剪贴板
        public StickerViewModel(Bitmap bitmap)
            : this(bitmap, new StickerStateData(SaveImageFromBitmap(bitmap), false), null, false, true)
        { }
        // 来自截图
        public StickerViewModel(Bitmap bitmap, Point initPoint)
            : this(bitmap, new StickerStateData(SaveImageFromBitmap(bitmap), false), initPoint, false, false)
        { }
        // 启动还原：从 STMP 已有文件加载，不再复制
        public StickerViewModel(StickerStateData state)
            : this(LoadBitmapFromFile(Path.Join(UserSettingData.Default.StickerFolderPath, state.ImageFileName)),
                   state, null, true, false)
        { }

        private StickerViewModel(Bitmap bitmap, StickerStateData state, Point? initPoint, bool isRestore, bool canZoomInInitially)
        {
            _sourceBitmap = bitmap;
            _state = state;
            _initPoint = initPoint;
            _isRestore = isRestore;
            _canZoomInInitially = canZoomInInitially;
            _originalImageSource = ImageUtility.BitmapToBitmapImage(bitmap);
            ImageSource = _originalImageSource;

            using var thumb = ImageUtility.Resize(bitmap, 175);
            ThumbnailSource = ImageUtility.BitmapToBitmapImage(thumb);

            _wrapper = new StickerViewModelWrapper(this);
            Instances.Add(_wrapper);

            if (!isRestore)
                RegisterNewSticker();
        }

        private static Bitmap LoadBitmapFromFile(string imagePath)
        {
            using var fib = FreeImageBitmap.FromFile(imagePath);
            return fib.ToBitmap();
        }

        // 把图库原文件复制到 STMP，保留原格式，返回 STMP 内文件名
        private static string SaveImageFromFile(string imagePath)
        {
            var fileName = Guid.NewGuid().ToString("N") + Path.GetExtension(imagePath);
            File.Copy(imagePath, Path.Join(UserSettingData.Default.StickerFolderPath, fileName));
            return fileName;
        }

        // 把内存位图以 PNG 写入 STMP，返回 STMP 内文件名
        private static string SaveImageFromBitmap(Bitmap bitmap)
        {
            var fileName = Guid.NewGuid().ToString("N") + ".png";
            bitmap.Save(Path.Join(UserSettingData.Default.StickerFolderPath, fileName), ImageFormat.Png);
            return fileName;
        }

        private string ImageFilePath => Path.Join(UserSettingData.Default.StickerFolderPath, _state.ImageFileName);

        // ── 状态持久化 ───────────────────────────────────────────────────────

        // 新建贴片
        private void RegisterNewSticker()
        {
            UserSettingData.Default.Stickers.Add(_state.ImageFileName);
        }

        // ── View 生命周期 ────────────────────────────────────────────────────

        protected override void OnViewLoaded()
        {

            // 还原时，若上次折叠，则按上次裁剪区域生成局部小图
            if (_isRestore)
            {
                if (_state.IsFolded)
                {
                    int cropW = Math.Min(_sourceBitmap.Width, (int)(64 / StickerState.ZoomRate));
                    int cropH = Math.Min(_sourceBitmap.Height, (int)(64 / StickerState.ZoomRate));
                    var croppedBitmap = new CroppedBitmap(_originalImageSource, new Int32Rect(_state.FoldCropX, _state.FoldCropY, cropW, cropH));
                    ImageSource = croppedBitmap;
                }
            }

            if (_initPoint != null)
            {
                // 只有在这个时候，设置 Left/Top ，才会同时被DPI转换。
                _state.Top = _initPoint.Value.Y;
                _state.Left = _initPoint.Value.X;
            }

            // 初始缩放：如果图片尺寸大于屏幕尺寸的 90%，则按比例缩小到屏幕内
            if (_canZoomInInitially)
            {
                var view = (StickerView)View;
                view.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, () =>
                {
                    var actualHeight = view.StickerImage.ActualHeight;
                    var actualWidth = view.StickerImage.ActualWidth;
                    var interopHelper = new WindowInteropHelper(view);
                    var screen = System.Windows.Forms.Screen.FromHandle(interopHelper.Handle);
                    var screenWidth = screen.WorkingArea.Width;
                    var screenHeight = screen.WorkingArea.Height;
                    DpiScale dpiInfo = VisualTreeHelper.GetDpi(view);
                    double scaleX = dpiInfo.DpiScaleX;
                    double scaleY = dpiInfo.DpiScaleY;
                    double wpfWidth = screenWidth / scaleX;
                    double wpfHeight = screenHeight / scaleY;
                    if (actualWidth > wpfWidth * 0.9 || actualHeight > wpfHeight * 0.9)
                    {
                        // Adjust the size or position if needed
                        double widthRatio = wpfWidth / actualWidth;
                        double heightRatio = wpfHeight / actualHeight;
                        double zoomFactor = Math.Min(widthRatio, heightRatio) * 0.9; // Slightly smaller than the screen
                        StickerState.ZoomRate = zoomFactor;
                    }
                });
            }
        }

        protected override void OnClose()
        {
            _state.Flush();
            _sourceBitmap.Dispose();
        }

        // ── {s:Action} 命令与事件方法 ────────────────────────────────────────

        public void CloseWindow()
        {
            // 用户主动关闭单张贴片：移出清单
            UserSettingData.Default.Stickers.Remove(_state.ImageFileName);
            RequestClose();
        }

        public void CopyImage()
        {
            if (_state.IsFolded) return;
            Clipboard.SetImage(ImageUtility.BitmapToBitmapImage(_sourceBitmap));
        }

        public void MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (_state.IsFolded) return;
            var modifiers = Keyboard.Modifiers;
            if (modifiers.HasFlag(ModifierKeys.Control))
                OpacityInner(_state.WindowOpacity + e.Delta / 2000.0);
            else if (modifiers.HasFlag(ModifierKeys.Shift))
                RotationInner(_state.RotationAngle + e.Delta / 120.0);
            else
                ZoomInner(_state.ZoomRate + e.Delta / 5000.0);
        }

        public void MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_state.IsFolded)
                Expand();
            else
                Fold(e);
        }

        public void IncreaseZoom(string zoomRate)
        { if (_state.IsFolded) return; ZoomInner(_state.ZoomRate + double.Parse(zoomRate)); }
        public void ZoomTo(string zoomRate)
        { if (_state.IsFolded) return; ZoomInner(double.Parse(zoomRate)); }

        public void IncreaseRotate(string rotationStr)
        { if (_state.IsFolded) return; RotationInner(_state.RotationAngle + double.Parse(rotationStr)); }
        public void ResetRotation()
        { if (_state.IsFolded) return; RotationInner(0); }

        public void HorizontalFlip()
        { if (_state.IsFolded) return; _state.IsFlippedH = !_state.IsFlippedH; }
        public void VerticalFlip()
        { if (_state.IsFolded) return; _state.IsFlippedV = !_state.IsFlippedV; }

        public void IncreaseOpacity(string opacityStr)
        { if (_state.IsFolded) return; OpacityInner(_state.WindowOpacity + double.Parse(opacityStr)); }
        public void SetOpacity(string opacityStr)
        { if (_state.IsFolded) return; OpacityInner(double.Parse(opacityStr)); }

        public void AddToDatabase()
        {
            if (_state.IsFolded) return;
            // 图片已落盘到 STMP，直接复用，无需再写临时文件
            var rootViewModel = (Application.Current.MainWindow as Window)?.DataContext as RootViewModel;
            rootViewModel?.AddPicturesInner([ImageFilePath]);
        }

        public void FocusAndExpand()
        {
            if (_state.IsFolded)
                Expand();
            var view = (StickerView)View;
            view.Activate();
        }

        // ── 内部辅助 ────────────────────────────────

        private void ZoomInner(double rate)
        {
            var minRate = 40 / Math.Min(_sourceBitmap.Width, _sourceBitmap.Height);
            _state.ZoomRate = Math.Max(rate, minRate);
        }

        private void OpacityInner(double opacity)
        {
            _state.WindowOpacity = Math.Clamp(opacity, 0.05, 1.0);
        }

        private void RotationInner(double angle)
        {
            _state.RotationAngle = angle % 360;
        }

        // ── 折叠/展开 ────────────────────────────────────────────────────────

        private void Fold(MouseButtonEventArgs e)
        {
            // view
            var view = (StickerView)View;

            var clickInImage = e.GetPosition(view.StickerImage);
            // 转换为像素坐标
            int pixelX = (int)(clickInImage.X / view.StickerImage.ActualWidth * _sourceBitmap.Width);
            int pixelY = (int)(clickInImage.Y / view.StickerImage.ActualHeight * _sourceBitmap.Height);
            int cropW = Math.Min(_sourceBitmap.Width, (int)(64 / StickerState.ZoomRate));
            int cropH = Math.Min(_sourceBitmap.Height, (int)(64 / StickerState.ZoomRate));
            pixelX = Math.Clamp(pixelX - cropW / 2, 0, _sourceBitmap.Width - cropW);
            pixelY = Math.Clamp(pixelY - cropH / 2, 0, _sourceBitmap.Height - cropH);
            _state.FoldCropX = pixelX;
            _state.FoldCropY = pixelY;
            var croppedBitmap = new CroppedBitmap(_originalImageSource, new Int32Rect(pixelX, pixelY, cropW, cropH));
            ImageSource = croppedBitmap;
            _state.IsFolded = true;

            // 计算折叠后窗口的边界框尺寸，考虑旋转角度
            double rad = _state.RotationAngle * Math.PI / 180.0;
            double cosA = Math.Abs(Math.Cos(rad));
            double sinA = Math.Abs(Math.Sin(rad));
            double bbW = croppedBitmap.Width * cosA + croppedBitmap.Height * sinA;
            double bbH = croppedBitmap.Width * sinA + croppedBitmap.Height * cosA;
            bbW = bbW * _state.ZoomRate + 4; //px 边框为 2px，四周共 4px
            bbH = bbH * _state.ZoomRate + 4; //px

            // 计算折叠后窗口位置偏移量，使点击点位于折叠后窗口中心
            var clickInWindow = e.GetPosition(view);
            _state.FoldOffsetX = clickInWindow.X - bbW / 2;
            _state.FoldOffsetY = clickInWindow.Y - bbH / 2;
            _state.Left += _state.FoldOffsetX;
            _state.Top += _state.FoldOffsetY;
        }

        private void Expand()
        {
            ImageSource = _originalImageSource;
            _state.IsFolded = false;

            _state.Left -= _state.FoldOffsetX;
            _state.Top -= _state.FoldOffsetY;

        }
    }
}