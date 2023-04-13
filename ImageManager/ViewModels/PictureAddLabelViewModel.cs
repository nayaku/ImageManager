using ImageManager.Data;
using ImageManager.Data.Model;
using Stylet;
using StyletIoC;
using System.Drawing;
using System.Windows;
using System.Windows.Input;

namespace ImageManager.ViewModels
{
    public class PictureAddLabelViewModel : Screen
    {
        [Inject]
        public ImageContext Context { get; set; }
        public string SearchText { get; set; }
        public bool ShowLabelPopup { get; set; }
        public List<Label> SearchedLabels { get; set; }
        public PictureAddLabelViewModel()
        {
        }
        public void UpdateSearchedLabels()
        {
            SearchText = SearchText?.Trim();
            if (SearchText == null || SearchText == "")
                SearchedLabels = Context.Labels.OrderByDescending(l => l.Num).ToList();
            else
                SearchedLabels = Context.Labels.Where(l => l.Name.Contains(SearchText)).OrderByDescending(l => l.Num).ToList();
        }
        public void LabelClick(Label label)
        {
            SearchText = label.Name;
        }
        public void SearchBarGotFocus()
        {
            UpdateSearchedLabels();
            Task.Run(async () =>
            {
                await Task.Delay(100);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ShowLabelPopup = true;
                });
            });
        }
        public void SearchBarLostFocus()
        {
            Task.Run(async () =>
            {
                await Task.Delay(100);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ShowLabelPopup = false;
                });
            });
        }
        public void WindowMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // 消除焦点
            Keyboard.ClearFocus();
            FocusManager.SetFocusedElement((DependencyObject)sender, (IInputElement)sender);
        }
        public void WindowKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
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