using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ImageManager
{
    public partial class CreateOrEditLabelDialog : Form
    {

        public CreateOrEditLabelDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 通过字符串新建一个标签
        /// </summary>
        /// <param name="labelName"></param>
        public CreateOrEditLabelDialog(string labelName) : this()
        {
            skinTextBox1.Text = Utils.ConvertToValidString(labelName);

        }

        /// <summary>
        /// 通过现用的标签进行修改
        /// </summary>
        /// <param name="imageLabel"></param>
        public CreateOrEditLabelDialog(ImageLabel imageLabel):this()
        {
            colorSelectedPanel.BackColor = imageLabel.Color;
            skinColorSelectPanel1.SelectedColor = imageLabel.Color;
            skinTextBox1.Text = imageLabel.Name;
            // 不可修改
            skinTextBox1.Enabled = false;
            ImageLabel = imageLabel;
        }

        public ImageLabel ImageLabel { get; private set; }

        public string GetName()
        {
            return skinTextBox1.Text;

        }
        public Color GetColor()
        {
            return skinColorSelectPanel1.SelectedColor;

        }


        private void SkinButton1_Click(object sender, EventArgs e)
        {
            
            // 创建标签模式
            if (ImageLabel == null)
            {
                ValidateChildren();
                var errorMsg = errorProvider1.GetError(skinTextBox1);
                if (errorMsg != "")
                {
                    var result = MessageBox.Show(this, errorMsg, "", MessageBoxButtons.OK,
               MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                    //result.HasFlag(DialogResult.Yes);
                    return;
                }
                if (Dao.GetImageLabel(GetName())!=null)
                {
                    var result = MessageBox.Show(this,$"标签已经存在，请更换标签名字！", "", MessageBoxButtons.OK,
               MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                    //result.HasFlag(DialogResult.Yes);
                    return;
                    
                }
                Dao.CreateImageLabel(GetName(), GetColor());
                ImageLabel = Dao.GetImageLabel(GetName());
            }
            // 修改模式
            else
            {
                if (!GetColor().Equals(ImageLabel.Color))
                {
                    Dao.ResetImageLabelColor(ImageLabel, GetColor());
                }
            }
            DialogResult = DialogResult.OK;
        }

        private void SkinButton2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void SkinColorSelectPanel1_SelectedColorChanged(object sender, EventArgs e)
        {
            colorSelectedPanel.BackColor = skinColorSelectPanel1.SelectedColor;
        }

        private void SkinTextBox1_Validated(object sender, EventArgs e)
        {
            var text = skinTextBox1.Text.Trim();
            skinTextBox1.Text = text;
            if (text == "")
            {
                errorProvider1.SetError(skinTextBox1, "标签名称不得为空！");
            }
            else if (!Utils.IsValidString(text))
            {
                errorProvider1.SetError(skinTextBox1, "标签名称不得包含" + Utils.InvalidChars.ToString()+"之类字符！");
            }
            else if (text.Length > 16)
            {
                errorProvider1.SetError(skinTextBox1, "标签名称长度不得大于16！");
            }
            else
            {
                errorProvider1.SetError(skinTextBox1,"");
            }
            
        }
    }
}
