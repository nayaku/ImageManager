using ImageManager.Data;
using ImageManager.Data.Model;
using StyletIoC;
using System.Windows.Input;

namespace ImageManager.ViewModels
{
    public class PictureAddLabelViewModel : Screen
    {
        [Inject]
        public ImageContext Context { get; set; }
        public string SearchText { get; set; } = string.Empty;
        public bool ShowLabelPopup { get; set; }
        public List<Label> SearchedLabels { get; set; }
        public PictureAddLabelViewModel()
        {
        }
        public void UpdateSearchedLabels()
        {
            var query = Context.Labels.AsQueryable();
            var searchText = SearchText?.Trim();
            if (!string.IsNullOrEmpty(searchText))
                query = query.Where(l => l.Name.Contains(searchText));
            SearchedLabels = query.OrderByDescending(l => l.Num).ToList();
            ShowLabelPopup = true;
        }
        public void LabelClick(Label label)
        {
            SearchText = label.Name;
            ShowLabelPopup = false;
        }
        public void SearchBarGotFocus()
        {
            UpdateSearchedLabels();
        }
        public void SearchBarLostFocus()
        {
            ShowLabelPopup = false;
        }
        private void ClearSerchBarFocus()
        {
            // 消除焦点
            Keyboard.ClearFocus();
            FocusManager.SetFocusedElement(View, View);
        }
        public void WindowMouseDown()
        {
            // 消除焦点
            ClearSerchBarFocus();
        }
        public void SearchBarKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                // 取消搜索
                SearchText = string.Empty;
                ClearSerchBarFocus();
            }
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
            SearchText = SearchText.Trim();
            if (ok && SearchText != string.Empty)
                RequestClose(ok);
            else
                RequestClose(false);
        }
    }
}