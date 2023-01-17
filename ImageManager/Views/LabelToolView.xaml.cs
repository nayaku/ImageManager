using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ImageManager.Views
{
    /// <summary>
    /// LabelToolView.xaml 的交互逻辑
    /// </summary>
    public partial class LabelToolView : HandyControl.Controls.Window
    {
        public LabelToolView()
        {
            InitializeComponent();
        }

        private void SearchBar_SearchStarted(object sender, HandyControl.Data.FunctionEventArgs<string> e)
        {

        }

        private void Tag_Selected(object sender, EventArgs e)
        {

        }

        private void Tag_Selected_1(object sender, EventArgs e)
        {

        }
    }
}
