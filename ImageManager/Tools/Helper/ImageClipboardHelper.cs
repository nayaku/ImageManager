using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ImageManager.Tools.Helper
{
    /// <summary>
    /// 把图片投放到剪贴板：同时提供多种格式，兼顾透明通道与老程序兼容性
    /// </summary>
    public static class ImageClipboardHelper
    {
        /// <summary>
        /// 复制图片到剪贴板。
        /// 同时写入 PNG（保留透明，Photoshop / 浏览器 / Office 优先取用）
        /// 与 CF_BITMAP（白底合成，供画图、聊天软件等只认位图的程序取用）
        /// </summary>
        public static void SetImage(Bitmap bitmap)
        {
            var dataObject = new DataObject();

            // PNG：带 alpha 通道。流不能提前释放，需等 SetDataObject 渲染完毕
            using var pngStream = new MemoryStream();
            bitmap.Save(pngStream, ImageFormat.Png);
            pngStream.Position = 0;
            dataObject.SetData("PNG", pngStream, false);

            // CF_BITMAP：位图没有 alpha，透明区直接放会变黑，故先合成白底
            using var opaque = FlattenOnWhite(bitmap);
            dataObject.SetData(DataFormats.Bitmap, ToBitmapSource(opaque), true);

            Clipboard.SetDataObject(dataObject, true);
        }

        /// <summary>
        /// 把图片合成到白色背景上，得到不含 alpha 的副本
        /// </summary>
        private static Bitmap FlattenOnWhite(Bitmap source)
        {
            var flatten = new Bitmap(source.Width, source.Height, PixelFormat.Format32bppRgb);
            using var g = Graphics.FromImage(flatten);
            g.Clear(Color.White);
            g.DrawImage(source, new Rectangle(0, 0, source.Width, source.Height));

            return flatten;
        }

        /// <summary>
        /// Bitmap --> BitmapSource。
        /// 这里必须走解码器返回 BitmapFrame，不能用 <see cref="BitmapImage"/>——
        /// 后者的 Metadata 属性无条件抛异常，剪贴板写入时会直接失败
        /// </summary>
        private static BitmapSource ToBitmapSource(Bitmap bitmap)
        {
            using var stream = new MemoryStream();
            bitmap.Save(stream, ImageFormat.Bmp);
            stream.Position = 0;
            var frame = BitmapFrame.Create(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
            frame.Freeze();

            return frame;
        }
    }
}
