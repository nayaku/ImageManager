using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageManager.Data.Model
{
    public class Workspace
    {
        [Key]
        public string Name { get; set; }
        [Required]
        public int Index { get; set; }

        public virtual List<WorkspacePicture> WorkspacePictures { get; set; }
    }
}
