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
    /// <summary>
    /// 数值输入对话框
    /// </summary>
    public partial class NumberInputDialog : Form
    {
        /// <summary>
        /// 数值输入对话框
        /// </summary>
        public NumberInputDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 数值输入对话框
        /// </summary>
        /// <param name="min">最小值</param>
        /// <param name="max">最大值</param>
        /// <param name="value">初始数值</param>
        /// <param name="decimalPlaces">小数位</param>
        /// <param name="increment">步进</param>
        public NumberInputDialog(decimal min,decimal max,decimal value,int decimalPlaces=0,decimal increment = 1):this()
        {
            skinNumericUpDown.Maximum = max;
            skinNumericUpDown.Minimum = min;
            skinNumericUpDown.Value = value;
            skinNumericUpDown.DecimalPlaces = decimalPlaces;
            skinNumericUpDown.Increment = increment;
        }

        /// <summary>
        /// 获取输入的数值
        /// </summary>
        /// <returns></returns>
        public decimal GetValue()
        {
            return skinNumericUpDown.Value;
        }
        
        private void OkSkinButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void CancelSkinButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}
