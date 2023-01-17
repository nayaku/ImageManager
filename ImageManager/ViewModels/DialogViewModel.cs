using HandyControl.Data;
using Stylet;
using System.Windows.Input;

namespace ImageManager.ViewModels
{
    public class DialogViewModel : Screen
    {
        public string Title { get; set; } = "Title";
        public string Message { get; set; } = "Message";
        public string ConfirmText { get; set; } = "确  定";
        public string CancelText { get; set; } = "取  消";
        public bool ShowCancel { get; set; }
        public string ConfirmButtonStyle { get; set; } = "ButtonPrimary";
        public string CancelButtonStyle { get; set; } = "ButtonDefault";
        public void WindowKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                OK("True");
            }
        }
        public void OK(string okString)
        {
            var ok = bool.Parse(okString);
            RequestClose(ok);
        }
    }
}