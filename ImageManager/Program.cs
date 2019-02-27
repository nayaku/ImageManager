using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImageManager
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var form = MainForm.GetInstance();
            Application.Run(form);
            //Application.Run(new StickerForm(@"E:\收集作品\图片\se兽耳\黒蜜大樹　商の枝.jpg"));
        }
    }
}
