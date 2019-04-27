using ImageManager.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ImageManager
{
    public partial class StickerForm : Form
    {
        /// <summary>
        /// 原始图片
        /// </summary>
        private Image _sourceImage;
        /// <summary>
        /// 缩放量
        /// </summary>
        private float _zoomRate = 1.0f;
        /// <summary>
        /// 最小缩放量
        /// </summary>
        private float _minRate = 0.1f;
        /// <summary>
        /// 鼠标按下的位置
        /// </summary>
        private Point _mousePoint;
        /// <summary>
        /// 是否被按下
        /// </summary>
        private bool _isPress = false;
        /// <summary>
        /// 时候被折叠
        /// </summary>
        private bool _isFolded = false;
        /// <summary>
        /// 源图时候过大
        /// </summary>
        private bool _isTooLarge = false;
        /// <summary>
        /// 图片路径（如果从从文件中读取图片的话）
        /// </summary>
        private string _imagePath = null;


        public StickerForm(string imgPath) : this(ImageReaderFactory.GetInstance().CreateImageReader(imgPath).Read(imgPath))
        {
            //saveImageToolStripMenuItem.Enabled = false;
            _imagePath = imgPath;
            // 判断是否原图过大。如果过大，用压缩过的图片替换源。
            if (_imagePath != null && Settings.Default.StickerUseMemoryOptimization && _sourceImage.Width > Screen.PrimaryScreen.Bounds.Width && _sourceImage.Height > Screen.PrimaryScreen.Bounds.Height)
            {
                var rate = Math.Max(1f * Screen.PrimaryScreen.Bounds.Width / _sourceImage.Width, 1f * Screen.PrimaryScreen.Bounds.Height / _sourceImage.Height);
                _sourceImage = SuperImageReader.ZoomImage(_sourceImage, rate);
                _zoomRate = 1f * pictureBox.Image.Width / _sourceImage.Width;
                _isTooLarge = true;
            }
        }

        public StickerForm(Image image)
        {
            InitializeComponent();

            _sourceImage = image;
            //saveImageToolStripMenuItem.Enabled = true;

            pictureBox.Image = (Image)image.Clone();

            // 图片尺寸过大，则缩小
            if (Screen.PrimaryScreen.Bounds.Width * 0.6f < image.Width || Screen.PrimaryScreen.Bounds.Height * 0.6f < image.Height)
            {
                _zoomRate = Math.Min(Screen.PrimaryScreen.Bounds.Width * 0.6f / image.Width, Screen.PrimaryScreen.Bounds.Height * 0.6f / image.Height);
                ZoomImage();
            }
            else
            {
                Size = pictureBox.Size = pictureBox.Image.Size;
            }


            //设置最小缩放
            var minWidth = 50f / _sourceImage.Size.Width;
            var minHeight = 50f / _sourceImage.Size.Height;
            _minRate = Math.Max(minHeight, minWidth);

            //滚轮监听                
            pictureBox.MouseWheel += new MouseEventHandler(this.PictureBox_MouseWhell);

        }

        /// <summary>
        /// 所有贴片实例
        /// </summary>
        public static List<StickerForm> InstanceList { get; } = new List<StickerForm>();

        private Dictionary<StickerForm, ToolStripMenuItem> _stickerFormDict = new Dictionary<StickerForm, ToolStripMenuItem>();
        /// <summary>
        /// 添加实例
        /// </summary>
        /// <param name="stickerForm"></param>
        private void AddInstance(StickerForm stickerForm)
        {
            var toolStripMenuItem = new ToolStripMenuItem
            {
                Image = SuperImageReader.ZoomImageWithHeight((Image)stickerForm._sourceImage.Clone(), 175),
                ImageScaling = ToolStripItemImageScaling.None
            };
            toolStripMenuItem.Click += stickerForm.OneStickerToolStripMenuItem_Click;
            stickerListToolStripMenuItem.DropDownItems.Add(toolStripMenuItem);
            _stickerFormDict[stickerForm] = toolStripMenuItem;
        }

        private void RemoveInstance(StickerForm stickerForm)
        {
            var toolStripMenuItem = _stickerFormDict[stickerForm];
            stickerListToolStripMenuItem.DropDownItems.Remove(toolStripMenuItem);
            _stickerFormDict.Remove(stickerForm);
        }

        /// <summary>
        /// 折叠贴片
        /// </summary>
        /// <param name="point">中心位置</param>
        public void FoldSticker(Point point = new Point())
        {
            if (_isFolded == false)
            {
                //var point = e.Location;
                var imageSize = pictureBox.Image.Size;
                if (point.X < 25) point.X = 25;
                if (point.Y < 25) point.Y = 25;
                if (imageSize.Width - point.X < 25) point.X = imageSize.Width - 25;
                if (imageSize.Height - point.Y < 25) point.Y = imageSize.Height - 25;
                var pictureLocation = point;
                pictureLocation.X = -point.X + 25;
                pictureLocation.Y = -point.Y + 25;
                pictureBox.Location = pictureLocation;
                Size = new Size(50, 50);
                ContextMenuStrip = null;
                // 设置小窗口的位置为鼠标点击位置
                var mouseLocation = Location;
                mouseLocation.X += point.X - 25;
                mouseLocation.Y += point.Y - 25;
                Location = mouseLocation;
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
                // 按照原先位置展开
                var formLocation = Location;
                formLocation.X += pictureBox.Location.X;
                formLocation.Y += pictureBox.Location.Y;
                Location = formLocation;

                pictureBox.Location = new Point(0, 0);
                Size = pictureBox.Size = pictureBox.Image.Size;
                ContextMenuStrip = contextMenuStrip;

                _isFolded = false;
            }
        }

        /// <summary>
        /// 重新计算缩放比率
        /// </summary>
        /// <param name="oldSourceImage">旧源图片</param>
        /// <param name="newSourceImage">新源图片</param>
        private void RecalculateZoomRate(Image oldSourceImage, Image newSourceImage)
        {
            _zoomRate = _zoomRate * oldSourceImage.Width / newSourceImage.Width;
        }

        /// <summary>
        /// 缩放图片
        /// </summary>
        private void ZoomImage()
        {
            if (_zoomRate < _minRate) _zoomRate = _minRate;

            var oldImage = pictureBox.Image;
            pictureBox.Image = SuperImageReader.ZoomImage((Image)_sourceImage.Clone(), _zoomRate);
            Size = pictureBox.Size = pictureBox.Image.Size;
            if (oldImage != _sourceImage) oldImage.Dispose();

        }

        private void RotateImage(RotateFlipType rotateFlipType)
        {
            var image = pictureBox.Image;
            image.RotateFlip(rotateFlipType);
            pictureBox.Image = image;
            Size = pictureBox.Size = image.Size;
        }

        private void CloseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _sourceImage.Dispose();
            Close();
        }

        private void CopyImageContentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_isTooLarge)
            {
                Clipboard.SetImage(ImageReaderFactory.GetInstance().CreateImageReader(_imagePath).Read(_imagePath));
            }
            else
            {
                Clipboard.SetImage(_sourceImage);
            }
        }

        private void SaveImageToolStripMenuItem_Click(object sender, EventArgs e)
        {


            //_sourceImage.Save(path);

        }

        private void EnlargeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _zoomRate += 0.1f;
            ZoomImage();
        }

        private void ShrinkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_zoomRate >= _minRate + 0.1f)
            {
                _zoomRate -= 0.1f;
                ZoomImage();
            }

        }

        private void ZoomTo10ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _zoomRate = 0.1f;
            ZoomImage();
        }

        private void ZoomTo50ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _zoomRate = 0.50f;
            ZoomImage();
        }

        private void ZoomTo100ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _zoomRate = 1.0f;
            ZoomImage();
        }

        private void ZoomTo200ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _zoomRate = 2.0f;
            ZoomImage();
        }

        private void ZoomToCustomerSetSizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var numberInputDialog = new NumberInputDialog(0, int.MaxValue, new decimal(_zoomRate), 2, new decimal(0.05));
            var dialogResult = numberInputDialog.ShowDialog(this);

            //判断返回值
            if (dialogResult == DialogResult.OK)
            {
                _zoomRate = (float)numberInputDialog.GetValue();
            }
            ZoomImage();
            numberInputDialog.Dispose();
        }

        private void Rotate90FilpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RotateImage(RotateFlipType.Rotate90FlipNone);

        }

        private void Rotate180FlipToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RotateImage(RotateFlipType.Rotate180FlipNone);
        }

        private void Rotate270FlipToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RotateImage(RotateFlipType.Rotate270FlipNone);
        }

        private void VerticalFlipToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RotateImage(RotateFlipType.RotateNoneFlipY);
        }

        private void HorizontalFilpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RotateImage(RotateFlipType.RotateNoneFlipX);
        }

        private void Opacity10ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Opacity = 0.1;
        }

        private void Opacity50ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Opacity = 0.5;
        }

        private void Opacity100ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Opacity = 1;
        }

        private void OpacityIncreaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Opacity += 0.01;
        }

        private void OpacityDecreaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Opacity >= 0.01)
            {
                Opacity -= 0.01;
            }
        }

        private void OpacityCustomerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var numberInputDialog = new NumberInputDialog(new decimal(0.1f), new Decimal(1), new decimal(Opacity), 2, new decimal(0.1));
            var dialogResult = numberInputDialog.ShowDialog(this);

            if (dialogResult == DialogResult.OK)
            {
                Opacity = (double)numberInputDialog.GetValue();
            }
            numberInputDialog.Dispose();
        }

        private void StickerForm_Resize(object sender, EventArgs e)
        {
            //待定
        }

        private void PictureBox_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (_isFolded)
            {
                ExpandSticker();
            }
            else
            {
                FoldSticker(e.Location);
            }
        }

        private void PictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            _mousePoint = MousePosition;
            _isPress = true;
        }

        private void PictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isPress)
            {
                var location = Location;
                location.X += MousePosition.X - _mousePoint.X;
                location.Y += MousePosition.Y - _mousePoint.Y;
                Location = location;
                _mousePoint = MousePosition;
            }

        }

        private void PictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            _isPress = false;
        }

        private void PictureBox_MouseWhell(object sender, MouseEventArgs e)
        {
            _zoomRate += e.Delta * 0.0001f;
            if (_zoomRate <= 0) _zoomRate = 0.01f;
            ZoomImage();
        }

        private void OneStickerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_isFolded)
            {
                ExpandSticker();
            }
            MouseTool.SetCursorPos(Location.X + Width / 2, Location.Y + Height / 2);
        }


        private void StickerForm_Shown(object sender, EventArgs e)
        {
            Size = pictureBox.Size = pictureBox.Image.Size;
            // 添加到实例列表
            InstanceList.Add(this);
            foreach (var stickerForm in InstanceList)
            {
                AddInstance(stickerForm);
                if (stickerForm != this)
                    stickerForm.AddInstance(this);
            }

        }

        private void StickerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            foreach (var stickerForm in InstanceList)
            {
                stickerForm.RemoveInstance(this);
            }
            InstanceList.Remove(this);
        }
    }
}
