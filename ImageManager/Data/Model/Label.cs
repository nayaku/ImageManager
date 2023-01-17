using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageManager.Data.Model
{
    [Index(nameof(Name),IsUnique =true)]
    public class Label
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public int Num { get; set; }
        public string NumToString => "(" + Num + ")";
        [NotMapped]
        public bool IsEdit { get; set; }
    }
}
