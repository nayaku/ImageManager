using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ImageManager.ViewModels
{
    public class PictureLabelWrapper : PropertyChangedBase
    {
        public Label Label { get; set; }
        public bool IsEdit { get; set; }
        public PictureLabelWrapper(Label label)
        {
            Label = label;
        }
    }
}
