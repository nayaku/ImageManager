using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImageManager
{
    public partial class ChooseLabelDialog : Form
    {
        public ChooseLabelDialog()
        {
            InitializeComponent();
            SkinComboBox_TextUpdate(this, new EventArgs());
        }

        /// <summary>
        /// 返回被选中的标签
        /// </summary>
        /// <returns></returns>
        public ImageLabel ImageLabel
        {
            get;
            private set;
        }

        private void SkinComboBox_TextUpdate(object sender, EventArgs e)
        {
            int selectionStart = skinComboBox.SelectionStart;
            var labels = Dao.GetImageLabels(skinComboBox.Text);
            //string[] userLabels = LabelData.GetInstance().SearchUserLabel(skinComboBox1.Text);
            skinComboBox.Items.Clear();
            if (labels.Length == 0)
            {
                skinComboBox.Items.Add("");
            }
            else
            {
                foreach (var label in labels)
                {
                    skinComboBox.Items.Add(label);
                }
            }

            if (!skinComboBox.DroppedDown)
                skinComboBox.DroppedDown = true;
            Cursor.Current = Cursors.Default;

            skinComboBox.SelectionStart = selectionStart;
        }

        private void OkSkinButton_Click(object sender, EventArgs e)
        {
            var text = skinComboBox.Text.Trim();
            if(text == "")
            {
                MessageBox.Show(this, $"标签名不得为空！", "", MessageBoxButtons.OK,
               MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return;
            }
            skinComboBox.SelectedItem = ImageLabel = Dao.GetImageLabel(skinComboBox.Text);
            skinComboBox.Text = text;
            if (ImageLabel == null)
            {
                var label = Dao.GetImageLabel(text);
                var result = MessageBox.Show(this, $"标签不存在，是否创建一个新的标签？", "", MessageBoxButtons.YesNo,
               MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                // 创建标签
                if (result==DialogResult.Yes)
                {
                    var coeDialog = new CreateOrEditLabelDialog(text);
                    var dialogResult = coeDialog.ShowDialog(this);
                    if (dialogResult==DialogResult.OK)
                    {
                        ImageLabel = coeDialog.ImageLabel;
                        coeDialog.Dispose();
                    }
                    else
                    {
                        coeDialog.Dispose();
                        return;
                    }
                }
                else
                {
                    return;
                }

            }
            DialogResult = DialogResult.OK;
        }

        private void CancelSkinButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void AddImageLabelSkinButton_Click(object sender, EventArgs e)
        {
            var coeDialog = new CreateOrEditLabelDialog(skinComboBox.Text);
            var dialogResult = coeDialog.ShowDialog(this);
            if (dialogResult==DialogResult.OK)
            {
                skinComboBox.Text = coeDialog.ImageLabel.Name;
            }
            coeDialog.Dispose();

        }

        private void skinComboBox_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (skinComboBox.SelectedItem != null)
            {
                var text = skinComboBox.SelectedItem.ToString();
                skinComboBox.SelectedItem = null;
                skinComboBox.Text = text;
            }

        }
    }
}
