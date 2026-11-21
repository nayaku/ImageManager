using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
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
        /// </summary>
        public static void SetImage(Bitmap bitmap)
        {
            var dataObject = new DataObject();

            // PNG：带 alpha 通道。流不可释放：若 SetDataObject 中途失败，
            // 剪贴板仍持有本 DataObject，之后别的程序取数据时还会读这个流
            var pngStream = new MemoryStream();
            bitmap.Save(pngStream, ImageFormat.Png);
            pngStream.Position = 0;
            dataObject.SetData("PNG", pngStream, false);

            // 查看网络上其他项目，大多只放了一个png进去；比如:
            // https://github.com/d2phap/ImageGlass/blob/642f0d791671c9d6f1b54f15074bf5c03fae62a1/v9/Components/ImageGlass.Base/WinApi/ClipboardEx.cs#L30
            // https://github.com/mono/mono/blob/main/mcs/class/System.Windows.Forms/System.Windows.Forms/Clipboard.cs
            // 因此这里注释掉

            // // CF_BITMAP：位图没有 alpha，透明区直接放会变黑，故先合成白底
            // using var opaque = FlattenOnWhite(bitmap);
            // dataObject.SetData(DataFormats.Bitmap, ToBitmapSource(opaque), true);

            Clipboard.SetDataObject(dataObject, true);
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetOpenClipboardWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        /// <summary>
        /// 查询当前打开着剪贴板的进程，用于 CLIPBRD_E_CANT_OPEN 时定位占用者
        /// </summary>
        public static string GetClipboardOpener()
        {
            var hwnd = GetOpenClipboardWindow();
            if (hwnd == IntPtr.Zero)
                return "无（已释放，或占用者未关联窗口）";
            GetWindowThreadProcessId(hwnd, out var pid);
            try
            {
                using var process = Process.GetProcessById((int)pid);
                return $"{process.ProcessName} (PID {pid})";
            }
            catch (Exception e) when (e is ArgumentException or InvalidOperationException)
            {
                return $"PID {pid}（进程已退出）";
            }
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
