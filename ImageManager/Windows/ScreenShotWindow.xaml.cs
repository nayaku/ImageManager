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
    public partial class ScreenShotWindow : Window
    {
        public Bitmap ScreenShootBitmap { get; set; }
        private double x;
        private double y;
        private Rectangle cropRectangle;
        private Bitmap gfxScreenShoot;
        private bool firstDown = true;
        private bool isMouseDown = false;
        // 全屏显示
        private int minX, minY, totalWidth, totalHeight;


        public ScreenShotWindow()
        {
            ScreenShoot();
            InitializeComponent();
        }

        ~ScreenShotWindow()
        {
            gfxScreenShoot?.Dispose();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            AdjustWindowSize();
            AddScreenShootToCanvas();
        }

        /// <summary>
        /// 截屏
        /// </summary>
        private void ScreenShoot()
        {
            // 获取所有显示器的工作区域
            var screens = DisplayAPI.EnumerateDisplays() ?? throw new Exception("获取显示器信息失败！");
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
            //gfxScreenShoot.SetResolution(96, 96); // 设置为标准DPI，避免后续处理时出现问题
            using (var gfx = Graphics.FromImage(gfxScreenShoot))
            {
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
        }

        // 导入Win32 API
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
            // 获取当前DPI并调整Bitmap的DPI以正确显示
            var dpi = VisualTreeHelper.GetDpi(this);
            gfxScreenShoot.SetResolution((float)(96 * dpi.DpiScaleX), (float)(96 * dpi.DpiScaleY));
            var bitmapImage = ImageUtility.BitmapToBitmapImage(gfxScreenShoot);
            CaptureCanvas.Children.Add(new System.Windows.Controls.Image()
            {
                Source = bitmapImage,
                Stretch = Stretch.None
            });
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
                return;
            isMouseDown = true;
            x = e.GetPosition(this).X;
            y = e.GetPosition(this).Y;
            if (firstDown)
            {
                cropRectangle = new Rectangle()
                {
                    Stroke = System.Windows.Media.Brushes.Red,
                    StrokeThickness = 1,
                    Fill = System.Windows.Media.Brushes.Transparent
                };
                CaptureCanvas.Children.Add(cropRectangle);
                firstDown = false;
            }
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (isMouseDown)
            {
                var width = Math.Abs(e.GetPosition(CaptureCanvas).X - x);
                var height = Math.Abs(e.GetPosition(CaptureCanvas).Y - y);
                cropRectangle.Width = width;
                cropRectangle.Height = height;
                Canvas.SetLeft(cropRectangle, Math.Min(x, e.GetPosition(CaptureCanvas).X));
                Canvas.SetTop(cropRectangle, Math.Min(y, e.GetPosition(CaptureCanvas).Y));
            }
        }

        private void CropBitmap()
        {
            // 获取当前窗口缩放因子
            DpiScale dpiScale = VisualTreeHelper.GetDpi(this);
            var scaleX = dpiScale.DpiScaleX;
            var scaleY = dpiScale.DpiScaleY;

            var left = (int)(Canvas.GetLeft(cropRectangle) * scaleX);
            var top = (int)(Canvas.GetTop(cropRectangle) * scaleY);
            var width = (int)(cropRectangle.Width * scaleX);
            var height = (int)(cropRectangle.Height * scaleY);
            Debug.WriteLine($"Cropping Bitmap at ({left}, {top}), Size: ({width}, {height})");

            ScreenShootBitmap = gfxScreenShoot.Clone(new System.Drawing.Rectangle(left, top, width, height), gfxScreenShoot.PixelFormat);
            var stickerWindow = new StickerWindow(ScreenShootBitmap)
            {
                // 设置窗口位置为截图时的位置，加上一定偏移量，避免找不到窗口
                Left = Canvas.GetLeft(cropRectangle) + Left + 10,
                Top = Canvas.GetTop(cropRectangle) + Top + 10
            };
            stickerWindow.Show();
        }

        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (isMouseDown)
            {
                isMouseDown = false;
                CropBitmap();
            }
            Close();
        }
    }
}
