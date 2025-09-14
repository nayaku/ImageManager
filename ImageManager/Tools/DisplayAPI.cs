using System.Runtime.InteropServices;

namespace ImageManager.Tools
{
    public class DisplayAPI
    {
        public class ScreenDisplayInfo
        {
            private IntPtr _handle;
            public string DeviceName { get; private set; } = string.Empty;
            public int Width { get; private set; }
            public int Height { get; private set; }
            public int Left { get; private set; }
            public int Top { get; private set; }
            public ScreenDisplayInfo(IntPtr handle)
            {
                _handle = handle;
                Init();
            }

            private void Init()
            {
                var monitorInfo = new MONITORINFOEX();
                monitorInfo.cbSize = Marshal.SizeOf(monitorInfo);

                // 获取显示器信息
                if (!GetMonitorInfo(_handle, ref monitorInfo))
                {
                    throw new Exception("获取显示器信息失败！");
                }

                // 获取显示器左上角坐标
                int left = monitorInfo.rcMonitor.left;
                int top = monitorInfo.rcMonitor.top;

                // 获取显示器设备上下文
                IntPtr hdc = CreateDC(null, monitorInfo.szDevice, null, IntPtr.Zero);
                if (hdc == IntPtr.Zero)
                {
                    throw new Exception($"获取显示器设备({monitorInfo.szDevice})上下文失败！");
                }

                // 获取逻辑分辨率
                int logicalWidth = GetDeviceCaps(hdc, HORZRES);
                int logicalHeight = GetDeviceCaps(hdc, VERTRES);

                //// 获取每英寸像素数
                //uint dpiX;
                //// 仅在Windows 8.1及以上版本支持
                //if (OperatingSystem.IsWindowsVersionAtLeast(6, 3)) // Windows 8.1是6.3
                //{
                //    if (GetDpiForMonitor(_handle, MonitorDpiType.MDT_EFFECTIVE_DPI, out dpiX, out _) != 0)
                //        throw new Exception($"获取显示器({monitorInfo.szDevice})DPI失败！");
                //}
                //else
                //{
                //    // 兼容旧版本系统，fallback到设备上下文方法
                //    dpiX = (uint)GetDeviceCaps(hdc, LOGPIXELSX);
                //    // 通常X和Y方向DPI相同
                //}

                //// 计算缩放因子
                //double scale = (double)dpiX / 96.0f;
                //Debug.WriteLine($"显示器设备: {monitorInfo.szDevice}, 逻辑分辨率: {logicalWidth}x{logicalHeight}, 缩放因子: {scale}, rcMonitor left:{left}, rcMonitor top:{top}, rcWork left:{monitorInfo.rcWork.left}, rcWork top:{monitorInfo.rcWork.top}");

                // 创建并填充ScreenDisplayInfo对象
                DeviceName = monitorInfo.szDevice;
                Width = logicalWidth;
                Height = logicalHeight;
                Left = left;
                Top = top;

                // 释放设备上下文
                DeleteDC(hdc);
            }
        }
        #region PInvoke Structures and Methods
        // 定义RECT结构体，用于表示矩形区域
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        // 定义MONITORINFOEX结构体，用于存储显示器信息
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct MONITORINFOEX
        {
            public int cbSize;
            public RECT rcMonitor;   // 显示器矩形区域
            public RECT rcWork;      // 工作区矩形区域
            public uint dwFlags;     // 显示器标志
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string szDevice;  // 设备名称
        }

        // 回调函数委托
        private delegate bool MonitorEnumProcDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData);

        [DllImport("shcore.dll")]
        private static extern int GetDpiForMonitor(IntPtr hmonitor, MonitorDpiType dpiType, out uint dpiX, out uint dpiY);

        [DllImport("user32.dll")]
        private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumProcDelegate lpfnEnum, IntPtr dwData);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFOEX lpmi);

        [DllImport("gdi32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr CreateDC(string lpszDriver, string lpszDevice, string lpszOutput, IntPtr lpInitData);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

        // 设备能力常量定义
        private const int HORZRES = 8;          // 逻辑宽度
        private const int VERTRES = 10;         // 逻辑高度
        private const int LOGPIXELSX = 88;     // 水平每英寸像素数
        private const int LOGPIXELSY = 90;     // 垂直每英寸像素数

        public enum MonitorDpiType
        {
            MDT_EFFECTIVE_DPI = 0, // 有效 DPI。 为缩放 UI 元素确定正确的比例系数时，应使用此值。 这包含用户为此特定显示设置的比例系数。
            MDT_ANGULAR_DPI = 1,   // 角度 DPI。 此 DPI 可确保在屏幕上以合规的角度分辨率呈现。 这不包括用户为此特定显示设置的比例系数。
            MDT_RAW_DPI = 2,       // 原始 DPI。 此值是屏幕本身测量的屏幕的线性 DPI。 如果要读取像素密度而不是建议的缩放设置，请使用此值。 这不包括用户为此特定显示器设置的比例系数，也不保证是受支持的 DPI 值。
            MDT_DEFAULT = MDT_EFFECTIVE_DPI
        }

        #endregion

        public static ScreenDisplayInfo[]? EnumerateDisplays()
        {
            var results = new List<ScreenDisplayInfo>();
            var callback = new MonitorEnumProcDelegate((IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData) =>
            {
                var screenInfo = new ScreenDisplayInfo(hMonitor);
                results.Add(screenInfo);
                return true; // 继续枚举
            });
            var ok = EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, callback, IntPtr.Zero);
            return ok ? [.. results] : null;
        }
    }
}
