using ImageManager.Properties;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace ImageManager
{
    public partial class ImageUserControl : UserControl
    {
        /// <summary>
        /// 图片名字标签被点击事件
        /// </summary>
        public EventHandler<ImageLabelUserControl> ImgLabelNameLabel_ClickEventHander;
        /// <summary>
        /// 内容发生变动事件
        /// </summary>
        public EventHandler<ImageUserControl> UpdateEventHander;
        public EventHandler<ImageUserControl> RemoveImageEventHandler;


        /// <param name="image">图片封装类</param>
        public ImageUserControl(MyImage image)
        {
            InitializeComponent();
            MyImage = image;
            UpdateTitle();
            UpdateImageLabel();

            MyImage.UpdateEventHandler = UpdateUserDefine;

            imageTitleLabel.MaximumSize = new Size(Settings.Default.ImageWidth, 0);

            BackColor = Color.FromArgb(150, 255, 255, 255);
            contextMenuStrip.BackColor = Color.FromArgb(100, 255, 255, 255);
        }

        /// <summary>
        /// 所属的图片封装类
        /// </summary>
        public MyImage MyImage
        {
            get;
        }

        ///// <summary>
        ///// 所属的图片
        ///// </summary>
        //public Image Image
        //{
        //    get;
        //    private set;
        //}

        /// <summary>
        /// 更新用户自定义组件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void UpdateUserDefine(object sender, EventArgs e)
        {
            //UpdateImage();
            UpdateTitle();
            UpdateImageLabel();
            UpdateEventHander(sender,this);
            Height = pictureBox.Height + imageTitleLabel.Height + imgLabelFlowLayoutPanel.Height;
        }

        /// <summary>
        /// 设置图片
        /// </summary>
        public void SetImage(Image image)
        {
            if (pictureBox.InvokeRequired)
            {

                pictureBox.Invoke((MethodInvoker)delegate
                {
                    SetImage(image);
                });
            }
            else
            {
                pictureBox.Image = image;
                pictureBox.ClientSize = pictureBox.Size = image.Size;

                Width = panel1.Width = imageTitleLabel.Width = image.Width;
                imageTitleLabel.MaximumSize = new Size(image.Width, imageTitleLabel.MaximumSize.Height);
                
                Height = pictureBox.Height + imageTitleLabel.Height + imgLabelFlowLayoutPanel.Height;
                //Height = image.Height + imageTitleLabel.MaximumSize.Height + imgLabelFlowLayoutPanel.Height;
            }
        }

        /// <summary>
        /// 更新标题
        /// </summary>
        public void UpdateTitle()
        {
            if (imageTitleLabel.InvokeRequired)
            {

                imageTitleLabel.Invoke((MethodInvoker)UpdateTitle);
            }
            else
            {
                imageTitleLabel.Text = MyImage.Title;
            }

        }

        /// <summary>
        /// 更新标签
        /// </summary>
        public void UpdateImageLabel()
        {
            if (imgLabelFlowLayoutPanel.InvokeRequired)
            {

                imgLabelFlowLayoutPanel.Invoke((MethodInvoker)UpdateImageLabel);
            }
            else
            {
                imgLabelFlowLayoutPanel.Controls.Clear();
                for (var index = 0; index < MyImage.GetImageLabelNum(); index++)
                {
                    var imageLabelUserControl = new ImageLabelUserControl(MyImage.GetImageLabel(index));
                    imageLabelUserControl.XLabel_ClickEventHandler = XLabel_Click;
                    imageLabelUserControl.ImgLabelNameLabel_ClickEventHander = ImgLabelNameLabel_Click;
                    imgLabelFlowLayoutPanel.Controls.Add(imageLabelUserControl);
                }
            }

        }

        /// <summary>
        /// 是否被选中
        /// </summary>
        public bool IsSelected()
        {
            return pictureCheckBox.Checked;
        }

        /// <summary>
        /// 设置选中状态
        /// </summary>
        /// <param name="state">true代表选中，false代表取消选中</param>
        public void SetSelected(bool state)
        {
            pictureCheckBox.Checked = state;
        }

        /// <summary>
        /// 给图片添加标签
        /// </summary>
        /// <param name="imageLabel"></param>
        public void AddImageLabel(ImageLabel imageLabel)
        {
            if (!MyImage.HasImageLabel(imageLabel))
            {
                MyImage.AddImageLabel(imageLabel);
            }
            //else
            //{
            //    MessageBox.Show(this, $"标签已经存在！", "", MessageBoxButtons.OK,
            //   MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            //}
            
        }

        #region 来自子元素ImageLabelUserControl 
        private void XLabel_Click(object sender, ImageLabelUserControl imageLabelUserControl)
        {
            Dao.UnlinkImageLabel(MyImage,imageLabelUserControl.ImageLabel);
            UpdateEventHander(sender,this);
        }

        private void ImgLabelNameLabel_Click(object sender, ImageLabelUserControl imageLabelUserControl)
        {
            ImgLabelNameLabel_ClickEventHander(sender,imageLabelUserControl);
        }
        #endregion

        private void PictureBox_Click(object sender, EventArgs e)
        {
            
            var stickerForm = new StickerForm(MyImage.Path);
            stickerForm.Show();
        }

        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PictureBox_Click(sender, e);
        }

        private void OpenWithOtherToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start(Utils.ConvertPath(MyImage.Path));
        }

        private void CopyImageContentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var image = ImageReaderFactory.GetInstance().CreateImageReader(Utils.ConvertPath(MyImage.Path)).Read(Utils.ConvertPath(MyImage.Path));
            Clipboard.SetImage(image);
        }

        private void CopyImagePathToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            var path = Utils.ConvertPath(MyImage.Path);
            Clipboard.SetText(path);
        }

        private void AddImgLabelToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            var chooseLabelDialog = new ChooseLabelDialog();
            var result = chooseLabelDialog.ShowDialog();
            if (result==DialogResult.OK)
            {
                AddImageLabel(chooseLabelDialog.ImageLabel);
            }
            chooseLabelDialog.Dispose();
        }

        private void RemoveImageToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Dao.RemoveImage(MyImage);
            RemoveImageEventHandler(sender,this);
        }
    }
}
