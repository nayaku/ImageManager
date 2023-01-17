using HandyControl.Controls;
using ImageManager.Data;
using ImageManager.Data.Model;
using Stylet;
using StyletIoC;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace ImageManager.ViewModels
{
    public class LabelToolViewModel : PropertyChangedBase
    {
        [Inject]
        public ImageContext Context { get; set; }
        public List<Label> SelectedLabels { get; set; }
        public string SearchText { get; set; }
        public List<Label> SearchedLabels { get; set; }
        public void SearchStart()
        {
            SearchedLabels = Context.Labels
                .Where(l => l.Name.Contains(SearchText))
                .ToList();
        }
        public void LabelSelected(object sender, EventArgs e)
        {
            var tag = (Tag)sender;
            if(tag.IsSelected)
                SelectedLabels.Add(SelectedLabels.Single(l => l.Name == tag.Content.ToString()));
            else
                SelectedLabels.Remove(SelectedLabels.Single(l => l.Name == tag.Content.ToString()));
        }

    }
}