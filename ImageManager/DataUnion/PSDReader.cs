using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.PSD;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageManager
{
    class PSDReader : SuperImageReader
    {
        public static string[] SupportImageTypes = { ".PSD"};

        public override Image Read(string path)
        {
            path = Utils.ConvertPath(path);
            try
            {
                var psdFile = new PsdFile();
                psdFile.Load(path);
                var image = ImageDecoder.DecodeImage(psdFile);
                return image;
            }
            catch(IOException e)
            {
                Debug.WriteLine(e);
                return null;
            }
        }
    }
}
