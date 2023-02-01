using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using Key = System.Windows.Input.Key;

namespace ImageManager.Tools
{
    public class HotKey
    {
        //简单说明一下：
        //“public static extern bool RegisterHotKey()”这个函数用于注册热键。由于这个函数需要引用user32.dll动态链接库后才能使用，并且
        //user32.dll是非托管代码，不能用命名空间的方式直接引用，所以需要用“DllImport”进行引入后才能使用。于是在函数前面需要加上
        //“[DllImport("user32.dll", SetLastError = true)]”这行语句。
        //“public static extern bool UnregisterHotKey()”这个函数用于注销热键，同理也需要用DllImport引用user32.dll后才能使用。
        //“public enum KeyModifiers{}”定义了一组枚举，将辅助键的数字代码直接表示为文字，以方便使用。这样在调用时我们不必记住每一个辅
        //助键的代码而只需直接选择其名称即可。

        //如果函数执行成功，返回值不为0。
        //如果函数执行失败，返回值为0。要得到扩展错误信息，调用GetLastError。
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool RegisterHotKey(
                        IntPtr hWnd,                //要定义热键的窗口的句柄
            int id,                     //定义热键ID（不能与其它ID重复）
            KeyModifiers fsModifiers,   //标识热键是否在按Alt、Ctrl、Shift、Windows等键时才会生效
            uint vk                   //定义热键的内容
            );
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool UnregisterHotKey(
            IntPtr hWnd,                //要取消热键的窗口的句柄
            int id                      //要取消热键的ID
            );
        //定义了辅助键的名称（将数字转变为字符以便于记忆，也可去除此枚举而直接使用数值）
        [Flags()]
        public enum KeyModifiers
        {
            None = 0,
            Alt = 1,
            Ctrl = 2,
            Shift = 4,
            WindowsKey = 8
        }

        const int WM_HOTKEY = 0x312;
        static int keyid = 10;
        static Dictionary<int, HotKeyCallBackHanlder> keymap = new();

        public delegate void HotKeyCallBackHanlder();

        /// <summary>
        /// 注册快捷键
        /// </summary>
        /// <param name="window">持有快捷键窗口</param>
        /// <param name="fsModifiers">组合键</param>
        /// <param name="key">快捷键</param>
        /// <param name="callBack">回调函数</param>
        /// <returns>注册成功</returns>
        public static bool Regist(Window window, KeyModifiers fsModifiers, Key key, HotKeyCallBackHanlder callBack)
        {
            var hwnd = new WindowInteropHelper(window).Handle;
            var _hwndSource = HwndSource.FromHwnd(hwnd);
            _hwndSource.AddHook(WndProc);

            int id = keyid++;

            var vk = KeyInterop.VirtualKeyFromKey(key);
            if (!RegisterHotKey(hwnd, id, fsModifiers, (uint)vk))
                return false;
            keymap[id] = callBack;
            return true;
        }

        /// <summary>
        /// 快捷键消息处理
        /// </summary>
        static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY)
            {
                int id = wParam.ToInt32();
                if (keymap.TryGetValue(id, out var callback))
                {
                    callback();
                }
            }
            return IntPtr.Zero;
        }

        /// <summary>
        /// 注销快捷键
        /// </summary>
        /// <param name="hWnd">持有快捷键窗口的句柄</param>
        /// <param name="callBack">回调函数</param>
        public static void UnRegist(IntPtr hWnd, HotKeyCallBackHanlder callBack)
        {
            foreach (KeyValuePair<int, HotKeyCallBackHanlder> var in keymap)
            {
                if (var.Value == callBack)
                    UnregisterHotKey(hWnd, var.Key);
            }
        }
    }
}
