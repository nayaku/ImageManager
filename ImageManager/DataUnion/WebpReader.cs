using Imazen.WebP;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageManager
{
    class WebpReader : SuperImageReader
    {
        public static string[] SupportImageTypes = { ".WEBP" };

        public override Image Read(string path)
        {
            path = Utils.ConvertPath(path);
            Imazen.WebP.Extern.LoadLibrary.LoadWebPOrFail();
            try
            {
                var data = File.ReadAllBytes(path);
                var decoder = new SimpleDecoder();
                var image = decoder.DecodeFromBytes(data, data.Length);
                return image;
            }
            catch (IOException e)
            {
                Debug.WriteLine(e);
                return null;
            }
        }
    }
}
