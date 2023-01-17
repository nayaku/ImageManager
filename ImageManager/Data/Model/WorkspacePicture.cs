using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageManager.Data.Model
{
    public class WorkspacePicture
    {
        public int Id { get; set; }
        public virtual Picture Picture { get; set; }
        public double Left { get; set; }
        public double Top { get; set; }
        public double ZoomRate { get; set; }
        public bool IsFolded { get; set; }
        public RotateFlipType RotateFlip { get; set; }
        public double Opacity { get; set; }
    }
}
