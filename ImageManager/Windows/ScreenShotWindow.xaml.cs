using HandyControl.Controls;
using ImageManager.Logging;
using ImageManager.Tools;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Rectangle = System.Windows.Shapes.Rectangle;

namespace ImageManager.Windows
{
    /// <summary>
    /// ScreenShotWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ScreenShotWindow
    {
        private static readonly Logger _logger = LoggerFactory.GetLogger(nameof(ScreenShotWindow));

        public static ScreenShotWindow? Instance { get; private set; }

        private double mouseDownX, mouseDownY;
        private Rectangle? cropRectangle;
        private Bitmap? gfxScreenShoot;
        // 全屏显示
        private int minX, minY, totalWidth, totalHeight;
        private readonly IWindowManager _windowManager;


        public static bool ShowScreenShotWindow(IWindowManager windowManager)
        {
            if (Instance != null)
                return false;
            var screenShotWindow = new ScreenShotWindow(windowManager);
            // 截屏失败就不显示窗口：全屏遮罩显示出来后再出错，整个桌面会被罩死
            if (!screenShotWindow.ScreenShoot())
            {
                screenShotWindow.Close();
                Growl.ErrorGlobal("截屏失败，请重试！");
                return false;
            }
            screenShotWindow.Show();
            Instance = screenShotWindow;
            return true;
        }

        private ScreenShotWindow(IWindowManager windowManager)
        {
            _windowManager = windowManager;
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            AdjustWindowSize();
            AddScreenShootToCanvas();
            // 确保窗口拿到键盘焦点，否则收不到 Esc
            Activate();
            Focus();
        }

        /// <summary>
        /// 截屏，须在窗口显示前调用。
        /// 返回是否成功；失败时不要显示窗口，否则全屏遮罩会罩死桌面
        /// </summary>
        private bool ScreenShoot()
        {
            try
            {
                CaptureScreens();
                return true;
            }
            catch (Exception ex)
            {
                // 显示器枚举失败、内存不足、锁屏或远程桌面下 CopyFromScreen 失败等
                _logger.Log(LogLevel.Error, exception: ex, report: true);
                gfxScreenShoot?.Dispose();
                gfxScreenShoot = null;
                return false;
            }
        }

        /// <summary>
        /// 抓取所有显示器画面到 <see cref="gfxScreenShoot"/>
        /// </summary>
        private void CaptureScreens()
        {
            // 获取所有显示器的工作区域
            var screens = DisplayAPI.EnumerateDisplays() ?? throw new Exception("获取显示器信息失败！");
            if (screens.Length == 0)
                throw new Exception("未检测到任何显示器！");
            minX = int.MaxValue;
            minY = int.MaxValue;
            totalWidth = 0;
            totalHeight = 0;
            // 获取最小值
            foreach (var screen in screens)
            {
                minX = Math.Min(minX, screen.Left);
                minY = Math.Min(minY, screen.Top);
            }
            // 计算总宽度和高度
            foreach (var screen in screens)
            {
                var width = screen.Width + screen.Left - minX;
                var height = screen.Height + screen.Top - minY;
                totalWidth = Math.Max(totalWidth, width);
                totalHeight = Math.Max(totalHeight, height);
            }
            // 创建一个Bitmap来存储截屏
            gfxScreenShoot = new Bitmap(totalWidth, totalHeight);
            using var gfx = Graphics.FromImage(gfxScreenShoot);
            // 设置高质量的绘图选项 
            gfx.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            gfx.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            gfx.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

            // 遍历每个显示器，截取其工作区域
            foreach (var screen in screens)
            {
                var size = new System.Drawing.Size(screen.Width, screen.Height);
                gfx.CopyFromScreen(screen.Left, screen.Top, screen.Left - minX, screen.Top - minY, size);
            }
        }

        #region SetWindowPos API
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(
            IntPtr hWnd,
            IntPtr hWndInsertAfter,
            int X,
            int Y,
            int cx,
            int cy,
            uint uFlags);

        // SetWindowPos的标志位
        private const uint SWP_NOZORDER = 0x0004;
        private const uint SWP_NOACTIVATE = 0x0010;
        private const uint SWP_SHOWWINDOW = 0x0040;
        #endregion

        /// <summary>
        /// 根据当前DPI调整窗口以覆盖所有显示器
        /// </summary>
        private void AdjustWindowSize()
        {
            // 调整窗口大小和位置
            IntPtr hWnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
            SetWindowPos(hWnd, IntPtr.Zero, minX, minY, totalWidth, totalHeight,
                SWP_NOZORDER | SWP_NOACTIVATE | SWP_SHOWWINDOW);
        }

        private void AddScreenShootToCanvas()
        {
            Debug.Assert(gfxScreenShoot != null);
            // 获取当前DPI并调整Bitmap的DPI以正确显示
            var dpi = VisualTreeHelper.GetDpi(this);
            gfxScreenShoot.SetResolution((float)(96 * dpi.DpiScaleX), (float)(96 * dpi.DpiScaleY));
            CaptureCanvas.Children.Add(new System.Windows.Controls.Image()
            {
                Source = ImageUtility.BitmapToBitmapSource(gfxScreenShoot),
                Stretch = Stretch.None
            });
        }

        private void CleanCrop()
        {
            if (cropRectangle != null)
            {
                CaptureCanvas.Children.Remove(cropRectangle);
                cropRectangle = null;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            // 在此释放截图位图，不用终结器：终结器里访问其他托管对象顺序不可控，
            // 且构造阶段失败时字段为 null，会在终结器线程抛异常直接终止进程
            gfxScreenShoot?.Dispose();
            gfxScreenShoot = null;
            if (ReferenceEquals(Instance, this))
                Instance = null;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                e.Handled = true;
                Close();
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
                return;
            CleanCrop();
            mouseDownX = e.GetPosition(this).X;
            mouseDownY = e.GetPosition(this).Y;
            cropRectangle = new Rectangle()
            {
                Stroke = System.Windows.Media.Brushes.Red,
                StrokeThickness = 1,
                Fill = System.Windows.Media.Brushes.Transparent
            };
            CaptureCanvas.Children.Add(cropRectangle);
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (cropRectangle != null)
            {
                var width = Math.Abs(e.GetPosition(CaptureCanvas).X - mouseDownX);
                var height = Math.Abs(e.GetPosition(CaptureCanvas).Y - mouseDownY);
                cropRectangle.Width = width;
                cropRectangle.Height = height;
                Canvas.SetLeft(cropRectangle, Math.Min(mouseDownX, e.GetPosition(CaptureCanvas).X));
                Canvas.SetTop(cropRectangle, Math.Min(mouseDownY, e.GetPosition(CaptureCanvas).Y));
            }
        }

        private void CropBitmap()
        {
            Debug.Assert(cropRectangle != null);
            Debug.Assert(gfxScreenShoot != null);

            // 获取当前窗口缩放因子
            DpiScale dpiScale = VisualTreeHelper.GetDpi(this);
            var scaleX = dpiScale.DpiScaleX;
            var scaleY = dpiScale.DpiScaleY;

            var left = (int)(Canvas.GetLeft(cropRectangle) * scaleX);
            var top = (int)(Canvas.GetTop(cropRectangle) * scaleY);
            var width = (int)(cropRectangle.Width * scaleX);
            var height = (int)(cropRectangle.Height * scaleY);
            if (width < 1 || height < 1)
            {
                Growl.ErrorGlobal("截图区域长宽不得小于1像素，请重新截图！");
                return;
            }
            Debug.WriteLine($"Cropping Bitmap at ({left}, {top}), Size: ({width}, {height})");

            var screenShootBitmap = gfxScreenShoot.Clone(new System.Drawing.Rectangle(left, top, width, height), gfxScreenShoot.PixelFormat);

            // 复制截图到剪贴板
            try
            {
                System.Windows.Forms.Clipboard.SetImage(screenShootBitmap);
                Growl.SuccessGlobal("截图已复制到剪贴板，可以直接粘贴使用！");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"复制截图到剪贴板失败: {ex.Message}");
                Growl.ErrorGlobal("复制截图到剪贴板失败，请重试！");
            }

            // 显示贴图窗口
            //var stickerWindow = new StickerWindow(screenShootBitmap)
            //{
            //    // 设置窗口位置为截图时的位置，加上一定偏移量，避免找不到窗口
            //    Left = Canvas.GetLeft(cropRectangle) + Left + 10,
            //    Top = Canvas.GetTop(cropRectangle) + Top + 10
            //};
            //stickerWindow.Show();
            // 设置窗口位置为截图时的位置，加上一定偏移量，避免找不到窗口
            var point = new System.Windows.Point(
                (Canvas.GetLeft(cropRectangle) + Left),
                (Canvas.GetTop(cropRectangle) + Top));
            var stickerViewModel = new ViewModels.StickerViewModel(screenShootBitmap, point);
            _windowManager.ShowWindow(stickerViewModel);
        }

        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (cropRectangle != null)
            {
                CropBitmap();
            }
            Close();
        }
    }
}
