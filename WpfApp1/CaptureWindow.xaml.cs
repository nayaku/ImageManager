using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace WPFCaptureScreenShot
{
    /// <summary>
    /// CaptureWindow.xaml 的交互逻辑
    /// </summary>
    public partial class CaptureWindow : Window
    {
        private double x;
        private double y;
        private double width;
        private double height;

        private bool isMouseDown = false;

        public Bitmap Bitmap { get; private set; }

        public CaptureWindow()
        {
            InitializeComponent();
        }

        private void CaptureWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed)
            {
                isMouseDown = true;
                x = e.GetPosition(null).X;
                y = e.GetPosition(null).Y;
            }
            else if(e.RightButton == MouseButtonState.Pressed)
            {
                DialogResult = false;
            }
            
        }

        private void CaptureWindow_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (isMouseDown)
            {
                // 通过一个矩形来表示目前截图区域
                System.Windows.Shapes.Rectangle rect = new System.Windows.Shapes.Rectangle();
                double dx = e.GetPosition(null).X;
                double dy = e.GetPosition(null).Y;
                double rectWidth = Math.Abs(dx - x);
                double rectHeight = Math.Abs(dy - y);
                SolidColorBrush brush = new SolidColorBrush(Colors.White);
                rect.Width = rectWidth;
                rect.Height = rectHeight;
                rect.Fill = brush;
                rect.Stroke = brush;
                rect.StrokeThickness = 1;
                if (dx < x)
                {
                    Canvas.SetLeft(rect, dx);
                    Canvas.SetTop(rect, dy);
                }
                else
                {
                    Canvas.SetLeft(rect, x);
                    Canvas.SetTop(rect, y);
                }

                CaptureCanvas.Children.Clear();
                CaptureCanvas.Children.Add(rect);

                if (e.LeftButton == MouseButtonState.Released)
                {
                    CaptureCanvas.Children.Clear();
                    // 获得当前截图区域
                    width = Math.Abs(e.GetPosition(null).X - x);
                    height = Math.Abs(e.GetPosition(null).Y - y);

                    if (e.GetPosition(null).X == x || e.GetPosition(null).Y == y)
                    {
                        DialogResult = false;
                    }
                    else if(e.GetPosition(null).X > x)
                    {
                        CaptureScreen(x, y, width, height);
                    }
                    else
                    {
                        CaptureScreen(e.GetPosition(null).X, e.GetPosition(null).Y, width, height);
                    }

                    isMouseDown = false;
                    DialogResult = true;
                    //x = 0.0;
                    //y = 0.0;
                    //this.Close();
                }
            }
        }

        private void CaptureScreen(double x, double y, double width, double height)
        {
            int ix = Convert.ToInt32(x);
            int iy = Convert.ToInt32(y);
            int iw = Convert.ToInt32(width);
            int ih = Convert.ToInt32(height);

            System.Drawing.Bitmap bitmap = new Bitmap(iw, ih);
            using (System.Drawing.Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.CopyFromScreen(ix, iy, 0, 0, new System.Drawing.Size(iw, ih));
                Bitmap = bitmap;
                //SaveFileDialog dialog = new SaveFileDialog();
                //dialog.Filter = "Png Files|*.png";
                //if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                //{
                //    bitmap.Save(dialog.FileName, ImageFormat.Png);
                //}
            }
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            DialogResult = false;
        }

        private void Window_Closed(object sender, EventArgs e)
        {

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //DialogResult = false;
        }
    }
}
