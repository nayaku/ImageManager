using ImageManager.Data;
using ImageManager.Data.Model;
using Stylet;
using StyletIoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageManager.ViewModels
{
    public class AddImageViewModel : PropertyChangedBase
    {
        [Inject]
        public ImageContext Context { get; set; }
        public string ScanResuleMessage { get; set; }
        public string Message { get; set; }
        public List<Picture> CanAddPictures { get; set; } = new();
        public List<Picture> SameConflictPictures { get; set; } = new();
        public List<Picture> SimilarConfictPictures { get; set; } = new();


        /// <param name="files">所有文件</param>
        /// <param name="pictures">图片实体</param>
        public AddImageViewModel(List<string> files, List<Picture> pictures)
        {
            foreach (var picture in pictures)
            {
                if (picture.SamePicture?.Count != 0)
                    SameConflictPictures.Add(picture);
                else if (picture.SimilarPictures?.Count != 0)
                    SimilarConfictPictures.Add(picture);
                else
                    CanAddPictures.Add(picture);
            }
            var message = $"一共扫描到{pictures.Count}张图片，其中有{SameConflictPictures.Count}张重复图片，" +
                $"有{SimilarConfictPictures.Count}张相似图片，此外还有{files.Count - pictures.Count}个文件不是图片。";
            Message = ScanResuleMessage = message;
        }
    }
}
