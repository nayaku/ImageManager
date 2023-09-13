using ImageManager.Tools.Extension;
using StyletIoC;
using System.Windows.Input;

namespace ImageManager.ViewModels
{
    public class DialogViewModel : Screen
    {
        public string Title { get; set; } = "Title";
        public string Message { get; set; } = "Message";
        public string ConfirmText { get; set; } = "Confirm";
        public string CancelText { get; set; } = string.Empty;
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
        public static bool? Show(IContainer container, string title, string message, string confirmText,
            string cancelText = "", string confirmButtonStyle = "ButtonPrimary",
            string cancelButtonStyle = "ButtonDefault")
        {
            var vm = new DialogViewModel
            {
                Title = title,
                Message = message,
                ConfirmText = confirmText,
                CancelText = cancelText,
                ConfirmButtonStyle = confirmButtonStyle,
                CancelButtonStyle = cancelButtonStyle
            };
            container.BuildUpEx(vm);
            var windowManager = container.Get<IWindowManager>();
            bool? res = null;
            Execute.OnUIThreadSync(() => res = windowManager.ShowDialog(vm));
            return res;
        }
    }
}