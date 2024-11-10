using FreeImageAPI;
using ImageManager.Tools;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace ImageManager.Windows
{
    /// <summary>
    /// StickerWindow.xaml 的交互逻辑
    /// </summary>
    public partial class StickerWindow
    {
        /// <summary>
        /// 原始图片
        /// </summary>
        private Bitmap _sourceImage;
        /// <summary>
        /// 缩放量
        /// </summary>
        private double _zoomRate = 1.0f;
        /// <summary>
        /// 鼠标按下的位置
        /// </summary>
        private System.Windows.Point _mousePoint;
        /// <summary>
        /// 是否被折叠
        /// </summary>
        private bool _isFolded = false;
        private RotateFlipType _rotateFlipType;

        /// <summary>
        /// 所有贴片实例
        /// </summary>
        private static List<StickerWindow> _instanceList { get; } = [];
        private Dictionary<StickerWindow, MenuItem> _stickerDict = [];

        public StickerWindow(string imagePath) : this(FreeImageBitmap.FromFile(imagePath).ToBitmap())
        {
        }
        public StickerWindow(Bitmap bitmap)
        {
            InitializeComponent();
            _sourceImage = bitmap;
            SetImage(bitmap);

            // 图片尺寸过大，则缩小
            if (SystemParameters.PrimaryScreenWidth * 0.6f < _sourceImage.Width || SystemParameters.PrimaryScreenHeight * 0.6f < _sourceImage.Height)
            {
                _zoomRate = Math.Min(SystemParameters.PrimaryScreenWidth * 0.6f / _sourceImage.Width, SystemParameters.PrimaryScreenHeight * 0.6f / _sourceImage.Height);
                RefreshSticker();
            }
        }

        /// <summary>
        /// 设置图片
        /// </summary>
        /// <param name="bitmap"></param>
        private void SetImage(Bitmap bitmap)
        {
            StickerImage.Source = ImageUtility.BitmapToBitmapImage(bitmap);
            StickerImage.Width = bitmap.Width;
            StickerImage.Height = bitmap.Height;
        }

        /// <summary>
        /// 刷新图片
        /// </summary>
        private void RefreshSticker()
        {
            var bitmap = _sourceImage;
            bitmap = ImageUtility.Resize(bitmap, _zoomRate);
            bitmap.RotateFlip(_rotateFlipType);
            SetImage(bitmap);
        }

        /// <summary>
        /// 添加实例
        /// </summary>
        /// <param name="stickerForm"></param>
        private void AddInstance(StickerWindow stickerWindow)
        {
            var menuItem = new MenuItem()
            {
                Header = new System.Windows.Controls.Image
                {
                    Source = ImageUtility.BitmapToBitmapImage(ImageUtility.Resize(stickerWindow._sourceImage, 175))
                },
            };
            menuItem.Click += stickerWindow.StickerMenuItem_Click;
            StickerList.Items.Add(menuItem);
            _stickerDict[stickerWindow] = menuItem;
        }

        /// <summary>
        /// 删除实例
        /// </summary>
        /// <param name="stickerWindow"></param>
        private void RemoveInstance(StickerWindow stickerWindow)
        {
            var menuItem = _stickerDict[stickerWindow];
            StickerList.Items.Remove(menuItem);
            _stickerDict.Remove(stickerWindow);
        }

        /// <summary>
        /// 折叠贴片
        /// </summary>
        /// <param name="point">中心位置</param>
        public void FoldSticker(System.Windows.Point point = new())
        {
            if (_isFolded == false)
            {
                var bitmap = ImageUtility.ImageSourceToBitmap(StickerImage.Source);
                var imageSize = StickerImage.DesiredSize;
                var img = ImageUtility.Crop(bitmap, (int)point.X, (int)point.Y, 50, 50);
                SetImage(img);
                if (point.X < 25) point.X = 25;
                if (point.Y < 25) point.Y = 25;
                if (imageSize.Width - point.X < 25) point.X = imageSize.Width - 25;
                if (imageSize.Height - point.Y < 25) point.Y = imageSize.Height - 25;

                // 设置小窗口的位置为鼠标点击位置
                _mousePoint = point;
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    Left += point.X - 25;
                    Top += point.Y - 25;
                }), null);
                _isFolded = true;
            }
        }

        /// <summary>
        /// 扩展贴片
        /// </summary>
        public void ExpandSticker()
        {
            if (_isFolded)
            {
                // 展开
                RefreshSticker();
                _isFolded = false;
                Left -= _mousePoint.X - 25;
                Top -= _mousePoint.Y - 25;
            }
        }

        private void Zoom(double rate)
        {
            _zoomRate = rate;
            // 设置缩放下限
            _zoomRate = Math.Max(_zoomRate, 40.0 / Math.Min(_sourceImage.Width, _sourceImage.Height));
            RefreshSticker();
        }

        #region 鼠标操作事件
        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_isFolded)
            {
                ExpandSticker();
            }
            else
            {
                FoldSticker(e.GetPosition(StickerImage));
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            // 按住Ctrl键改变透明度
            if (Keyboard.GetKeyStates(Key.LeftCtrl) == KeyStates.Down || Keyboard.GetKeyStates(Key.RightCtrl) == KeyStates.Down)
            {
                Opacity += e.Delta / 2000.0;
                Opacity = Math.Max(0.05, Math.Min(1, Opacity));
            }
            // 改变缩放比例
            else
            {
                var rate = _zoomRate + e.Delta / 5000.0;
                Zoom(rate);
            }

        }
        #endregion

        #region 窗口操作事件
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _instanceList.Add(this);
            foreach (var instance in _instanceList)
            {
                AddInstance(instance);
                if (instance != this)
                {
                    instance.AddInstance(this);
                }
            }
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            foreach (var instance in _instanceList)
            {
                instance.RemoveInstance(this);
            }
            _instanceList.Remove(this);
        }
        #endregion

        #region 右键菜单事件
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void StickerMenuItem_Click(object sender, EventArgs e)
        {
            if (_isFolded)
            {
                ExpandSticker();
            }
            MouseTool.SetCursorPos((int)(Left + Width / 2), (int)(Top + Height / 2));
        }

        private void CopyImage_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetImage(ImageUtility.BitmapToBitmapImage(_sourceImage));
        }


        private void ZoomOut_Click(object sender, RoutedEventArgs e)
        {
            Zoom(_zoomRate - 0.1);
        }

        private void ZoomIn_Click(object sender, RoutedEventArgs e)
        {
            Zoom(_zoomRate + 0.1);
        }

        private void Zoom10_Click(object sender, RoutedEventArgs e)
        {
            Zoom(0.1);
        }

        private void Zoom50_Click(object sender, RoutedEventArgs e)
        {
            Zoom(0.5);
        }

        private void Zoom100_Click(object sender, RoutedEventArgs e)
        {
            Zoom(1.0);
        }

        private void Zoom200_Click(object sender, RoutedEventArgs e)
        {
            Zoom(2);
        }

        private void Rotate90_Click(object sender, RoutedEventArgs e)
        {
            var angle = (int)_rotateFlipType & 3;
            angle = (angle + 1) % 4;
            _rotateFlipType = (RotateFlipType)(angle | (int)_rotateFlipType & ~3);
            RefreshSticker();

        }

        private void Rotate90t_Click(object sender, RoutedEventArgs e)
        {
            var angle = (int)_rotateFlipType & 3;
            angle = (angle + 4 - 1) % 4;
            _rotateFlipType = (RotateFlipType)(angle | (int)_rotateFlipType & ~3);
            RefreshSticker();
        }

        private void HorizontalFilp_Click(object sender, RoutedEventArgs e)
        {
            _rotateFlipType ^= RotateFlipType.RotateNoneFlipX;
            RefreshSticker();
        }

        private void VerticalFilp_Click(object sender, RoutedEventArgs e)
        {
            _rotateFlipType ^= RotateFlipType.RotateNoneFlipY;
            RefreshSticker();
        }

        private void Transparency10_Click(object sender, RoutedEventArgs e)
        {
            Opacity = 0.1;
        }

        private void Transparency50_Click(object sender, RoutedEventArgs e)
        {
            Opacity = 0.5;
        }

        private void Transparency100_Click(object sender, RoutedEventArgs e)
        {
            Opacity = 1;
        }

        private void TransparencyReduce_Click(object sender, RoutedEventArgs e)
        {
            Opacity = Math.Max(0, Opacity - 0.1);
        }

        private void TransparencyIncrease_Click(object sender, RoutedEventArgs e)
        {
            Opacity = Math.Min(1, Opacity + 0.1);
        }
        #endregion

    }
}
