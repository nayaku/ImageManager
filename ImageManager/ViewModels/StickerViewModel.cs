using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using FreeImageAPI;
using ImageManager.Data;
using ImageManager.Logging;
using ImageManager.Tools;
using ImageManager.Views;
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
        private double _dpiScaleX = 1.0;
        private double _dpiScaleY = 1.0;

        /// <summary>
        /// 主程序退出中：贴片关闭时只落盘、不清理文件，避免误删还原数据。
        /// </summary>
        public static bool IsShuttingDown { get; set; }

        public static ObservableCollection<StickerViewModelWrapper> Instances { get; } = [];

        public BitmapImage ImageSource { get; private set; }
        public BitmapImage ThumbnailSource { get; private set; }
        public double DisplayWidth { get; private set; }
        public double DisplayHeight { get; private set; }
        public double FlipScaleX { get; private set; } = 1.0;
        public double FlipScaleY { get; private set; } = 1.0;
        /// <summary>
        /// 窗口实际透明度
        /// 折叠时强制不透明，展开时取持久化透明度
        /// </summary>
        public double EffectiveOpacity { get; private set; } = 1.0;

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
                  new StickerStateData(SaveImageFromFile(imagePath), true), null, false)
        { }
        // 来自截图/剪贴板：编码为 PNG
        public StickerViewModel(Bitmap bitmap, Point? initPoint = null)
            : this(bitmap, new StickerStateData(SaveImageFromBitmap(bitmap), false), initPoint, false)
        { }
        // 启动还原：从 STMP 已有文件加载，不再复制
        public StickerViewModel(StickerStateData state)
            : this(LoadBitmapFromFile(Path.Join(UserSettingData.Default.StickerFolderPath, state.ImageFileName)),
                   state, null, true)
        { }

        private StickerViewModel(Bitmap bitmap, StickerStateData state, Point? initPoint, bool isRestore)
        {
            _sourceBitmap = bitmap;
            _state = state;
            _initPoint = initPoint;
            _isRestore = isRestore;
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
            var dpi = VisualTreeHelper.GetDpi(View);
            _dpiScaleX = dpi.DpiScaleX;
            _dpiScaleY = dpi.DpiScaleY;

            if (_isRestore)
            {
                ApplyRestoreState();
                return;
            }

            UpdateTransform();
            RefreshOpacity();
            if (_initPoint != null)
            {
                var view = (StickerView)View;
                view.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, () =>
                {
                    _state.Left = _initPoint.Value.X / _dpiScaleX;
                    _state.Top = _initPoint.Value.Y / _dpiScaleY;
                });
            }
        }

        // 按当前显示器 DPI 还原（缩放因子/裁剪为源像素，故物理尺寸与上次一致）
        private void ApplyRestoreState()
        {
            if (_state.IsFolded)
            {
                ApplyFoldedVisual();
            }
            else
            {
                UpdateTransform();
            }
            RefreshOpacity();
            // Left/Top/RotationAngle/IsFolded 已是 _state 的值，绑定自动生效
        }

        protected override void OnClose()
        {
            if (IsShuttingDown)
            {
                // 退出兜底：落盘最新状态，保留文件供下次还原
                _state.Flush();
                _sourceBitmap.Dispose();
                return;
            }

            // 用户主动关闭单张贴片：移出清单并删除其图片与状态文件
            _state.Flush();
            UserSettingData.Default.Stickers.Remove(_state.ImageFileName);
            DeleteStickerFiles();
            _sourceBitmap.Dispose();
            Instances.Remove(_wrapper);
        }

        private void DeleteStickerFiles()
        {
            try
            {
                if (File.Exists(_state.FilePath))
                    File.Delete(_state.FilePath);
                if (File.Exists(ImageFilePath))
                    File.Delete(ImageFilePath);
            }
            catch (Exception ex)
            {
                LoggerFactory.GetLogger(nameof(StickerViewModel)).Error(ex);
            }
        }

        // ── {s:Action} 命令与事件方法 ────────────────────────────────────────

        public void CloseWindow() => RequestClose();

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
            var view = (StickerView)View;
            if (_state.IsFolded)
                Expand();
            else
                Fold(e.GetPosition(view.StickerImage));
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
        { if (_state.IsFolded) return; _state.IsFlippedH = !_state.IsFlippedH; UpdateTransform(); }
        public void VerticalFlip()
        { if (_state.IsFolded) return; _state.IsFlippedV = !_state.IsFlippedV; UpdateTransform(); }

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
            var minRate = 40.0 / Math.Min(_sourceBitmap.Width, _sourceBitmap.Height);
            _state.ZoomRate = Math.Max(rate, minRate);
            UpdateTransform();
        }

        private void OpacityInner(double opacity)
        {
            _state.WindowOpacity = Math.Clamp(opacity, 0.05, 1.0);
            RefreshOpacity();
        }

        private void RotationInner(double angle)
        {
            _state.RotationAngle = angle % 360;
            UpdateTransform();
        }

        // ── 折叠/展开 ────────────────────────────────────────────────────────

        private void Fold(Point clickPosInDip)
        {
            if (_state.IsFolded) return;

            double pixelX = clickPosInDip.X * _dpiScaleX / _state.ZoomRate;
            double pixelY = clickPosInDip.Y * _dpiScaleY / _state.ZoomRate;

            var cropW = Math.Min(_sourceBitmap.Width, 64);
            var cropH = Math.Min(_sourceBitmap.Height, 64);
            var x = Math.Clamp((int)pixelX - cropW / 2, 0, _sourceBitmap.Width - cropW);
            var y = Math.Clamp((int)pixelY - cropH / 2, 0, _sourceBitmap.Height - cropH);
            _state.FoldCropX = x; _state.FoldCropY = y; _state.FoldCropW = cropW; _state.FoldCropH = cropH;

            ApplyFoldedVisual();
            _state.IsFolded = true;

            double foldedW = cropW / _dpiScaleX;
            double foldedH = cropH / _dpiScaleY;
            double rad = _state.RotationAngle * Math.PI / 180.0;
            double cosA = Math.Abs(Math.Cos(rad));
            double sinA = Math.Abs(Math.Sin(rad));
            double bbW = foldedW * cosA + foldedH * sinA + 2;
            double bbH = foldedW * sinA + foldedH * cosA + 2;

            RefreshOpacity();

            var view = (StickerView)View;
            var clickInWindow = Mouse.GetPosition(view);
            _state.FoldOffsetX = clickInWindow.X - bbW / 2;
            _state.FoldOffsetY = clickInWindow.Y - bbH / 2;
            _state.Left += _state.FoldOffsetX;
            _state.Top += _state.FoldOffsetY;
        }

        private void Expand()
        {
            if (!_state.IsFolded) return;

            ImageSource = _originalImageSource;
            _state.IsFolded = false;
            UpdateTransform();

            _state.Left -= _state.FoldOffsetX;
            _state.Top -= _state.FoldOffsetY;
            RefreshOpacity();
        }

        // 按 _state 中的折叠裁剪区域生成局部小图，并设置显示尺寸与翻转
        private void ApplyFoldedVisual()
        {
            using var cropped = ImageUtility.Crop(_sourceBitmap, _state.FoldCropX, _state.FoldCropY, _state.FoldCropW, _state.FoldCropH);
            ImageSource = ImageUtility.BitmapToBitmapImage(cropped);
            DisplayWidth = _state.FoldCropW / _dpiScaleX;
            DisplayHeight = _state.FoldCropH / _dpiScaleY;
            FlipScaleX = _state.IsFlippedH ? -1.0 : 1.0;
            FlipScaleY = _state.IsFlippedV ? -1.0 : 1.0;
        }

        private void UpdateTransform()
        {
            FlipScaleX = _state.IsFlippedH ? -1.0 : 1.0;
            FlipScaleY = _state.IsFlippedV ? -1.0 : 1.0;
            DisplayWidth = _sourceBitmap.Width * _state.ZoomRate / _dpiScaleX;
            DisplayHeight = _sourceBitmap.Height * _state.ZoomRate / _dpiScaleY;
        }

        private void RefreshOpacity()
        {
            EffectiveOpacity = _state.IsFolded ? 1.0 : _state.WindowOpacity;
        }
    }
}