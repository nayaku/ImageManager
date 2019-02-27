using ImageManager.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using static ImageManager.Dao;

namespace ImageManager
{
    /// <summary>
    /// 所有图像读取器的超类
    /// </summary>
    public abstract class SuperImageReader
    {
        /// <summary>
        /// 读取
        /// </summary>
        /// <returns></returns>
        public abstract Image Read(string path);

        /// <summary>
        /// 等比例缩放图片  
        /// </summary>
        /// <param name="bitmap">位图</param>
        /// <param name="destHeight">目标位图高度</param>
        /// <param name="destWidth">目标位图宽度</param>
        /// <returns></returns>
        public static Image ZoomImage(Image bitmap, int destHeight, int destWidth)
        {

            try
            {
                Image sourImage = bitmap;
                int width = destWidth, height = destHeight;

                //按比例缩放             
                int sourWidth = sourImage.Width;
                int sourHeight = sourImage.Height;

                Bitmap destBitmap = new Bitmap(destWidth, destHeight);
                Graphics g = Graphics.FromImage(destBitmap);
                g.Clear(Color.Transparent);
                //设置画布的描绘质量           
                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(sourImage, new Rectangle((destWidth - width) / 2, (destHeight - height) / 2, width, height), 0, 0, sourImage.Width, sourImage.Height, GraphicsUnit.Pixel);
                g.Dispose();
                //设置压缩质量       
                //System.Drawing.Imaging.EncoderParameters encoderParams = new System.Drawing.Imaging.EncoderParameters();
                //long[] quality = new long[1];
                //quality[0] = 100;
                //System.Drawing.Imaging.EncoderParameter encoderParam = new System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
                //encoderParams.Param[0] = encoderParam;
                sourImage.Dispose();
                return destBitmap;
            }
            catch
            {
                return bitmap;
            }
        }
        /// <summary>
        /// 使用设定的宽度缩放图片
        /// </summary>
        /// <param name="bitmap">图像</param>
        /// <param name="rate">比率</param>
        /// <returns></returns>
        public static Image ZoomImage(Image bitmap,float rate)
        {
            int width = (int)(rate * bitmap.Width);
            int height = (int)(rate * bitmap.Height);
            return ZoomImage(bitmap, height, width);
        }

        /// <summary>
        /// 使用设置的宽度缩放图片
        /// </summary>
        /// <param name="bitmap">图像</param>
        /// <returns></returns>
        public static Image ZoomImage(Image bitmap)
        {
            int width = Settings.Default.ImageWidth;
            if (width >= bitmap.Width)
                return bitmap;
            else
                return ZoomImage(bitmap, (int)(bitmap.Height * width / bitmap.Width), (int)width);
        }

        /// <summary>
        /// 使用指定的高度缩放图片
        /// </summary>
        /// <param name="bitmap">图像</param>
        /// <returns></returns>
        public static Image ZoomImageWithHeight(Image bitmap,int height)
        {
            if (height >= bitmap.Height)
                return bitmap;
            else
                return ZoomImage(bitmap,height, (int)(1.0*bitmap.Width * height / bitmap.Height));
        }
    }
}