using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ImageManager
{
    class ClearMemoryUtil
    {
        [DllImport("psapi.dll")]
        static extern int EmptyWorkingSet(IntPtr hwProc);

        /// <summary>
        /// 释放内存
        /// </summary>
        public static void Clear()
        {
            int successProcess = 0;
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Process[] processes = Process.GetProcesses();
            foreach (Process process in processes)
            {
                //对于系统进程会拒绝访问，导致出错，此处对异常不进行处理。
                //以下系统进程没有权限，所以跳过，防止出错影响效率。  
                if ((process.ProcessName == "System") && (process.ProcessName == "Idle"))
                    continue;
                
                try
                {
                    EmptyWorkingSet(process.Handle);
                    successProcess++;
                }
                catch
                {
                    
                }
            }
            Debug.WriteLine($"成功释放内存的进程数为{successProcess}个。");
        }
    }
}
