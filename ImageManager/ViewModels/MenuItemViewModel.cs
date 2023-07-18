using System.Windows.Input;

namespace ImageManager.ViewModels
{
    public class MenuItemViewModel
    {
        public bool IsSeparator { get; set; } = false;
        public object? Header { get; set; } = null;
        public ICommand? Command { get; set; } = null;
        public string InputGestureText { get; set; }=string.Empty;
    }
}
