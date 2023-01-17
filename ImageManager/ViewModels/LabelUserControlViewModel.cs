using ImageManager.Data;
using ImageManager.Data.Model;
using Stylet;
using StyletIoC;
using System.Windows;

namespace ImageManager.ViewModels
{
    public class LabelUserControlViewModel : PropertyChangedBase
    {
        public Label Label { get; set; }
        public bool CanDelete { get; set; }
        public EventHandler<LabelUserControlViewModel> LabelDeleteEvent { get; set; }
        
        public LabelUserControlViewModel(Label label, bool canDelete)
        {
            Label = label;
            CanDelete = canDelete;
        }

        public void TagClosed(object? sender)
        {
            LabelDeleteEvent?.Invoke(sender, this);
        }
    }
}