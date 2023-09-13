using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ImageManager.Views
{
    /// <summary>
    /// ProgressView.xaml 的交互逻辑
    /// </summary>
    public partial class ProgressView
    {
        public ProgressView()
        {
            InitializeComponent();
        }

        private void TextBlock_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            TextBlock textBlock = sender as TextBlock;
            Debug.WriteLine(textBlock.Text);
            if (textBlock != null)
            {
                string str = textBlock.Text;
                Typeface typeface = new Typeface(
                    textBlock.FontFamily,
                    textBlock.FontStyle,
                    textBlock.FontWeight,
                    textBlock.FontStretch);
                FormattedText formattedText = new FormattedText(
                    str,
                    System.Threading.Thread.CurrentThread.CurrentCulture,
                    textBlock.FlowDirection,
                    typeface,
                    textBlock.FontSize,
                    textBlock.Foreground);
                if (textBlock.DesiredSize.Width + 1 < formattedText.Width)
                {
                    string str1 = textBlock.Text;
                    int p1 = FindPosition(str1, typeface, textBlock);
                    char[] ret = str1.ToCharArray();
                    string str2 = string.Concat<char>(ret.Reverse<char>());
                    int p2 = FindPosition(str2, typeface, textBlock);
                    string strOutPut = str.Substring(0, p1) + "..." + str.Substring(str.Length - p2, p2);
                    textBlock.Text = strOutPut;
                }
            }

        }

        private int FindPosition(string str, Typeface typeface, TextBlock textBlock)
        {
            int start = 0, end = str.Length - 1;
            while (start <= end)
            {
                int mid = (start + end) / 2;
                string strTemp = str.Substring(0, mid);
                FormattedText formattedText = new FormattedText(
                    strTemp,
                    System.Threading.Thread.CurrentThread.CurrentCulture,
                    textBlock.FlowDirection,
                    typeface,
                    textBlock.FontSize,
                    textBlock.Foreground);
                if (formattedText.Width <= (textBlock.DesiredSize.Width - 10) / 2)
                    start = mid + 1;
                else
                    end = mid - 1;
            }
            if (start == 0)
                return -1;
            else
                return start - 1;
        }
    }
}
