using System.Windows.Input;

namespace ImageManager.Views
{
    public partial class StickerView : HandyControl.Controls.Window
    {
        public StickerView()
        {
            InitializeComponent();
        }

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
    }
}
