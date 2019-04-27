using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImageManager
{
    public partial class ImageLabelUserControl : UserControl
    {
        /// <summary>
        /// X标签被点击事件
        /// </summary>
        public EventHandler<ImageLabelUserControl> XLabel_ClickEventHandler;
        /// <summary>
        /// 图片名字标签被点击事件
        /// </summary>
        public EventHandler<ImageLabelUserControl> ImgLabelNameLabel_ClickEventHander;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="imgLabel">图片标签</param>
        public ImageLabelUserControl(ImageLabel imgLabel)
        {
            InitializeComponent();
            ImageLabel = imgLabel;
            UpdateImageLabel();
            imgLabel.UpdateEventHandler = UpdateImageLabel;

        }

        /// <summary>
        /// 所属的ImageLabel类
        /// </summary>
        public ImageLabel ImageLabel
        {
            get;
        }

        /// <summary>
        /// 更新标签
        /// </summary>
        public void UpdateImageLabel()
        {
            imgLabelNameLabel.Text = ImageLabel.ToString();
            skinPanel1.BackColor = ImageLabel.Color;
        }

        /// <summary>
        /// 更新标签
        /// </summary>
        public void UpdateImageLabel(object sender,EventArgs e)
        {
           
                imgLabelNameLabel.Invoke((MethodInvoker)delegate
                {
                    UpdateImageLabel();
                });
           
        }

        private void XLabel_Click(object sender, EventArgs e)
        {
            XLabel_ClickEventHandler?.Invoke(sender,this);
        }

        private void ImgLabelNameLabel_Click(object sender, EventArgs e)
        {
            ImgLabelNameLabel_ClickEventHander?.Invoke(sender, this);
        }

    }
}
