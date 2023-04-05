using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;

namespace ImageManager.Tools
{
    public class ImageUtility
    {
        /// <summary>
        /// Bitmap --> BitmapImage
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static BitmapImage BitmapToBitmapImage(Bitmap bitmap)
        {
            using var memory = new MemoryStream();
            bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Tiff);
            memory.Position = 0;
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = memory;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
            bitmapImage.Freeze();

            return bitmapImage;
        }

        // BitmapImage --> Bitmap
        public static Bitmap BitmapImageToBitmap(BitmapImage bitmapImage)
        {

            using var outStream = new MemoryStream();
            BitmapEncoder enc = new BmpBitmapEncoder();
            enc.Frames.Add(BitmapFrame.Create(bitmapImage));
            enc.Save(outStream);
            var bitmap = new Bitmap(outStream);

            return new Bitmap(bitmap);
        }
        // RenderTargetBitmap --> BitmapImage
        public static BitmapImage ConvertRenderTargetBitmapToBitmapImage(RenderTargetBitmap wbm)
        {
            BitmapImage bmp = new BitmapImage();
            using (MemoryStream stream = new MemoryStream())
            {
                BmpBitmapEncoder encoder = new BmpBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(wbm));
                encoder.Save(stream);
                bmp.BeginInit();
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                bmp.StreamSource = new MemoryStream(stream.ToArray()); //stream;
                bmp.EndInit();
                bmp.Freeze();
            }
            return bmp;
        }


        // RenderTargetBitmap --> BitmapImage
        public static BitmapImage RenderTargetBitmapToBitmapImage(RenderTargetBitmap rtb)
        {
            var renderTargetBitmap = rtb;
            var bitmapImage = new BitmapImage();
            var bitmapEncoder = new PngBitmapEncoder();
            bitmapEncoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));

            using (var stream = new MemoryStream())
            {
                bitmapEncoder.Save(stream);
                stream.Seek(0, SeekOrigin.Begin);

                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = stream;
                bitmapImage.EndInit();
            }

            return bitmapImage;
        }

        // ImageSource --> Bitmap
        public static Bitmap ImageSourceToBitmap(ImageSource imageSource)
        {
            BitmapSource m = (BitmapSource)imageSource;

            Bitmap bmp = new Bitmap(m.PixelWidth, m.PixelHeight, System.Drawing.Imaging.PixelFormat.Format32bppPArgb); // 坑点：选Format32bppRgb将不带透明度

            System.Drawing.Imaging.BitmapData data = bmp.LockBits(
            new Rectangle(System.Drawing.Point.Empty, bmp.Size), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

            m.CopyPixels(Int32Rect.Empty, data.Scan0, data.Height * data.Stride, data.Stride);
            bmp.UnlockBits(data);

            return bmp;
        }

        // BitmapImage --> byte[]
        public static byte[] BitmapImageToByteArray(BitmapImage bmp)
        {
            byte[] bytearray = null;
            try
            {
                Stream smarket = bmp.StreamSource; ;
                if (smarket != null && smarket.Length > 0)
                {
                    //设置当前位置
                    smarket.Position = 0;
                    using (BinaryReader br = new BinaryReader(smarket))
                    {
                        bytearray = br.ReadBytes((int)smarket.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return bytearray;
        }


        // byte[] --> BitmapImage
        public static BitmapImage ByteArrayToBitmapImage(byte[] array)
        {
            using (var ms = new MemoryStream(array))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad; // here
                image.StreamSource = ms;
                image.EndInit();
                image.Freeze();
                return image;
            }
        }

        public static Bitmap ConvertByteArrayToBitmap(byte[] bytes)
        {
            Bitmap img = null;
            try
            {
                if (bytes != null && bytes.Length != 0)
                {
                    MemoryStream ms = new MemoryStream(bytes);
                    img = new Bitmap(ms);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return img;
        }

        /// <summary>
        /// 比率缩放
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="targetHeight"></param>
        /// <returns></returns>
        public static Bitmap Resize(Bitmap bitmap, double rate)
        {
            var width = (int)(bitmap.Width * rate);
            var height = (int)(bitmap.Height * rate);
            return Resize(bitmap, width, height);
        }

        /// <summary>
        /// 保证宽高比例的一种压缩方式
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="targetHeight"></param>
        /// <returns></returns>
        public static Bitmap Resize(Bitmap bitmap, int targetHeight)
        {
            int targetWidth = bitmap.Width * targetHeight / bitmap.Height;
            return Resize(bitmap, targetWidth, targetHeight);
        }

        /// <summary>
        /// 直接Resize到给定大小
        /// </summary>
        /// <param name="input"></param>
        /// <param name="targetWidth"></param>
        /// <param name="targetHeight"></param>
        /// <returns></returns>
        public static Bitmap Resize(Bitmap input, int targetWidth, int targetHeight)
        {
            try
            {
                var actualBitmap = new Bitmap(targetWidth, targetHeight);

                var g = Graphics.FromImage(actualBitmap);
                //设置画布的描绘质量           
                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

                g.DrawImage(input,
                    new Rectangle(0, 0, targetWidth, targetHeight),
                    new Rectangle(0, 0, input.Width, input.Height),
                    GraphicsUnit.Pixel);
                g.Dispose();
                return actualBitmap;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"图片压缩异常{ex.Message}");
                return null;
            }
        }

        public static Bitmap Crop(Bitmap bitmap, int x, int y, int width, int height)
        {
            var rect = new Rectangle(x, y, width, height);
            return Crop(bitmap, rect);
        }
        /// <summary>
        /// 裁切图片
        /// </summary>
        /// <param name="src"></param>
        /// <param name="cropRect"></param>
        /// <returns></returns>
        public static Bitmap Crop(Bitmap src, Rectangle cropRect)
        {
            Bitmap target = new Bitmap(cropRect.Width, cropRect.Height);

            using (Graphics g = Graphics.FromImage(target))
            {
                g.DrawImage(src, new Rectangle(0, 0, target.Width, target.Height),
                      cropRect,
                      GraphicsUnit.Pixel);
            }
            return target;
        }
    }
}
