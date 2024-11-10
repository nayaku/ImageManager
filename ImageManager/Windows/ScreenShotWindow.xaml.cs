using ImageManager.Tools;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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


        public ScreenShotWindow()
        {
            InitializeComponent();
            ScreenShoot();
        }

        /// <summary>
        /// 截屏
        /// </summary>
        public void ScreenShoot()
        {
            var width = (int)SystemParameters.VirtualScreenWidth;
            var height = (int)SystemParameters.VirtualScreenHeight;
            var left = (int)SystemParameters.VirtualScreenLeft;
            var top = (int)SystemParameters.VirtualScreenTop;

            Width = SystemParameters.VirtualScreenWidth;
            Height = SystemParameters.VirtualScreenHeight;
            Left = SystemParameters.VirtualScreenLeft;
            Top = SystemParameters.VirtualScreenTop;

            // 获取截图
            gfxScreenShoot = new Bitmap(width, height);
            using Graphics gfx = Graphics.FromImage(gfxScreenShoot);
            gfx.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            gfx.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
            gfx.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            gfx.CopyFromScreen(left, top, 0, 0, new System.Drawing.Size(width, height), CopyPixelOperation.SourceCopy);

            var bitmapImage = ImageUtility.BitmapToBitmapImage(gfxScreenShoot);
            CaptureCanvas.Children.Add(new System.Windows.Controls.Image()
            {
                Source = bitmapImage
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
            var left = (int)Canvas.GetLeft(cropRectangle);
            var top = (int)Canvas.GetTop(cropRectangle);
            var width = (int)cropRectangle.Width;
            var height = (int)cropRectangle.Height;
            ScreenShootBitmap = gfxScreenShoot.Clone(new System.Drawing.Rectangle(left, top, width, height), gfxScreenShoot.PixelFormat);
            var stickerWindow = new StickerWindow(ScreenShootBitmap)
            {
                Left = left,
                Top = top
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
