using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using FreeImageAPI;
using ImageManager.Tools;
using ImageManager.Views;
using Path = System.IO.Path;
using Point = System.Windows.Point;
using Window = HandyControl.Controls.Window;

namespace ImageManager.ViewModels
{
    public class StickerViewModelWrapper : ICommand
    {
        private readonly StickerViewModel _sticker;
        public BitmapImage ThumbnailSource => _sticker.ThumbnailSource;

        public StickerViewModelWrapper(StickerViewModel sticker) => _sticker = sticker;

        bool ICommand.CanExecute(object? parameter) => true;
        void ICommand.Execute(object? parameter) => _sticker.FocusAndExpand();
        event EventHandler? ICommand.CanExecuteChanged { add { } remove { } }
    }

    public class StickerViewModel : Screen
    {
        private readonly Bitmap _sourceBitmap;
        private readonly BitmapImage _originalImageSource;
        private double _zoomRate = 1.0;
        private double _rotationAngle;
        private bool _isFlippedH;
        private bool _isFlippedV;
        private double _dpiScaleX = 1.0;
        private double _dpiScaleY = 1.0;
        private Point _foldOffsetPoint;
        private readonly Point? _initPoint;
        private double _windowOpacity = 1.0;
        private readonly StickerViewModelWrapper _wrapper;

        public static ObservableCollection<StickerViewModelWrapper> Instances { get; } = [];

        public BitmapImage ImageSource { get; private set; }
        public BitmapImage ThumbnailSource { get; private set; }
        public double DisplayWidth { get; private set; }
        public double DisplayHeight { get; private set; }
        public double RotationAngle { get; private set; }
        public double FlipScaleX { get; private set; } = 1.0;
        public double FlipScaleY { get; private set; } = 1.0;
        public double WindowOpacity { get; private set; } = 1.0;
        public bool IsFolded { get; private set; }
        public bool CanAddToDatabase { get; }

        public StickerViewModel(string imagePath) : this(LoadBitmapFromFile(imagePath), false) { }
        public StickerViewModel(Bitmap bitmap, Point? initPoint = null) : this(bitmap, true, initPoint) { }

        private StickerViewModel(Bitmap bitmap, bool canAddToDatabase, Point? initPoint = null)
        {
            _sourceBitmap = bitmap;
            CanAddToDatabase = canAddToDatabase;
            _initPoint = initPoint;
            _originalImageSource = ImageUtility.BitmapToBitmapImage(bitmap);
            ImageSource = _originalImageSource;

            using var thumb = ImageUtility.Resize(bitmap, 175);
            ThumbnailSource = ImageUtility.BitmapToBitmapImage(thumb);

            _wrapper = new StickerViewModelWrapper(this);
            Instances.Add(_wrapper);
        }

        private static Bitmap LoadBitmapFromFile(string imagePath)
        {
            using var fib = FreeImageBitmap.FromFile(imagePath);
            return fib.ToBitmap();
        }

        // ── View 生命周期 ────────────────────────────────────────────────────

        protected override void OnViewLoaded()
        {
            var dpi = VisualTreeHelper.GetDpi(View);
            _dpiScaleX = dpi.DpiScaleX;
            _dpiScaleY = dpi.DpiScaleY;
            UpdateTransform();

            if (_initPoint != null)
            {
                var view = (StickerView)View;
                view.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, () =>
                {
                    view.Left = _initPoint.Value.X / _dpiScaleX;
                    view.Top = _initPoint.Value.Y / _dpiScaleY;
                    //Debug.WriteLine($"Initial position: {_initPoint.Value}");
                    //Debug.WriteLine($"DPI Scale: {_dpiScaleX}, {_dpiScaleY}");
                });
            }
        }

        protected override void OnClose()
        {
            _sourceBitmap.Dispose();
            Instances.Remove(_wrapper);
        }

        // ── {s:Action} 命令与事件方法 ────────────────────────────────────────

        public void CloseWindow()
        {
            // 请求关闭当前View
            RequestClose();
        }

        public void CopyImage()
        {
            if (IsFolded) return;
            Clipboard.SetImage(ImageUtility.BitmapToBitmapImage(_sourceBitmap));
        }

        public void MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (IsFolded) return;
            var modifiers = Keyboard.Modifiers;
            if (modifiers.HasFlag(ModifierKeys.Control))
                OpacityInner(WindowOpacity + e.Delta / 2000.0);
            else if (modifiers.HasFlag(ModifierKeys.Shift))
                RotationInner(_rotationAngle + e.Delta / 120.0);
            else
                ZoomInner(_zoomRate + e.Delta / 5000.0);
        }

        public void MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var view = (StickerView)View;
            if (IsFolded)
            {
                Expand();
            }
            else
            {
                Fold(e.GetPosition(view.StickerImage));
            }
        }

        public void IncreaseZoom(string zoomRate)
        { if (IsFolded) return; ZoomInner(_zoomRate + double.Parse(zoomRate)); }
        public void ZoomTo(string zoomRate)
        { if (IsFolded) return; ZoomInner(double.Parse(zoomRate)); }
        public void ResetZoom()
        { if (IsFolded) return; ZoomInner(1.0); }
        public void IncreaseRotate(string rotationStr)
        { if (IsFolded) return; RotationInner(_rotationAngle + double.Parse(rotationStr)); }
        public void ResetRotation()
        { if (IsFolded) return; RotationInner(0); }

        public void HorizontalFlip()
        { if (IsFolded) return; _isFlippedH = !_isFlippedH; UpdateTransform(); }
        public void VerticalFlip()
        { if (IsFolded) return; _isFlippedV = !_isFlippedV; UpdateTransform(); }

        public void IncreaseOpacity(string opacityStr)
        { if (IsFolded) return; WindowOpacity = Math.Clamp(WindowOpacity + double.Parse(opacityStr), 0.05, 1.0); }
        public void SetOpacity(string opacityStr)
        { if (IsFolded) return; OpacityInner(double.Parse(opacityStr)); }
        public void ResetOpacity()
        { if (IsFolded) return; WindowOpacity = 1.0; }

        public void AddToDatabase()
        {
            if (IsFolded) return;
            var tempPath = Path.GetTempFileName() + ".png";
            _sourceBitmap.Save(tempPath, ImageFormat.Png);
            var rootViewModel = (Application.Current.MainWindow as Window)?.DataContext as RootViewModel;
            rootViewModel?.AddPicturesInner([tempPath]);
        }

        public void FocusAndExpand()
        {
            if (IsFolded)
            {
                Expand();
            }
            var view = (StickerView)View;
            view.Activate();
            // MouseTool.SetCursorPos(
            // (int)((view.Left + view.Width / 2) * _dpiScaleX),
            // (int)((view.Top + view.Height / 2) * _dpiScaleY));
        }

        // ── 内部辅助 ────────────────────────────────

        private void ZoomInner(double rate)
        {
            var minRate = 40.0 / Math.Min(_sourceBitmap.Width, _sourceBitmap.Height);
            _zoomRate = Math.Max(rate, minRate);
            UpdateTransform();
        }

        private void OpacityInner(double opacity)
        {
            WindowOpacity = Math.Clamp(opacity, 0.05, 1.0);
        }

        private void RotationInner(double angle)
        {
            _rotationAngle = angle % 360;
            UpdateTransform();
        }

        // ── 折叠/展开 ────────────────────────────────────────────────────────

        private void Fold(Point clickPosInDip)
        {
            if (IsFolded) return;

            double pixelX = clickPosInDip.X * _dpiScaleX / _zoomRate;
            double pixelY = clickPosInDip.Y * _dpiScaleY / _zoomRate;

            var cropW = Math.Min(_sourceBitmap.Width, 64);
            var cropH = Math.Min(_sourceBitmap.Height, 64);
            var x = Math.Clamp((int)pixelX - cropW / 2, 0, _sourceBitmap.Width - cropW);
            var y = Math.Clamp((int)pixelY - cropH / 2, 0, _sourceBitmap.Height - cropH);

            using var cropped = ImageUtility.Crop(_sourceBitmap, x, y, cropW, cropH);
            ImageSource = ImageUtility.BitmapToBitmapImage(cropped);
            DisplayWidth = cropW / _dpiScaleX;
            DisplayHeight = cropH / _dpiScaleY;
            RotationAngle = _rotationAngle;
            FlipScaleX = _isFlippedH ? -1.0 : 1.0;
            FlipScaleY = _isFlippedV ? -1.0 : 1.0;
            IsFolded = true;

            double foldedW = cropW / _dpiScaleX;
            double foldedH = cropH / _dpiScaleY;
            double rad = _rotationAngle * Math.PI / 180.0;
            double cosA = Math.Abs(Math.Cos(rad));
            double sinA = Math.Abs(Math.Sin(rad));
            double bbW = foldedW * cosA + foldedH * sinA + 2;
            double bbH = foldedW * sinA + foldedH * cosA + 2;

            _windowOpacity = WindowOpacity;
            WindowOpacity = 1;

            var view = (StickerView)View;
            var clickInWindow = Mouse.GetPosition(view);
            _foldOffsetPoint = new Point(clickInWindow.X - bbW / 2, clickInWindow.Y - bbH / 2);
            view.Left += _foldOffsetPoint.X; view.Top += _foldOffsetPoint.Y;
        }

        private void Expand()
        {
            if (!IsFolded) return;

            ImageSource = _originalImageSource;
            IsFolded = false;
            UpdateTransform();

            var view = (StickerView)View;
            view.Left -= _foldOffsetPoint.X; view.Top -= _foldOffsetPoint.Y;
            WindowOpacity = _windowOpacity;
        }

        private void UpdateTransform()
        {
            RotationAngle = _rotationAngle;
            FlipScaleX = _isFlippedH ? -1.0 : 1.0;
            FlipScaleY = _isFlippedV ? -1.0 : 1.0;
            DisplayWidth = _sourceBitmap.Width * _zoomRate / _dpiScaleX;
            DisplayHeight = _sourceBitmap.Height * _zoomRate / _dpiScaleY;
        }
    }
}
