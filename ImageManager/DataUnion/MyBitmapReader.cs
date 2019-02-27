using ImageManager.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace ImageManager
{
    /// <summary>
    /// 封装系统的Bitmap类
    /// </summary>
    public class MyBitmapReader : SuperImageReader
    {
        public static string []SupportImageTypes = {".BMP",".GIF",".EXIF",".JPEG",".JPG",".PNG",".TIFF"};
        public override Image Read(string path)
        {
            path = Utils.ConvertPath(path);
            try
            {
                Stream s = File.Open(path, FileMode.Open);
                var bitmap = Image.FromStream(s);
                s.Close();
                return bitmap;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return null;
            }
        }
    }
}