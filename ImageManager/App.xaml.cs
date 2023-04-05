﻿using HandyControl.Themes;
using ImageManager.Tools;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;

namespace ImageManager
{
    public partial class App : Application
    {
        public App()
        {
#if !DEBUG
            Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
#endif
        }
        internal void UpdateTheme(ApplicationTheme theme)
        {
            if (ThemeManager.Current.ApplicationTheme != theme)
            {
                ThemeManager.Current.ApplicationTheme = theme;
            }
        }
        private static Mutex mutex;
        //系统能够识别有名称的互斥，因此可以使用它禁止应用程序启动两次 
        //第二个参数可以设置为产品的名称:Application.ProductName 
        // 每次启动应用程序，都会验证名称为OnlyRun的互斥是否存在 
        protected override void OnStartup(StartupEventArgs e)
        {
            mutex = new Mutex(true, ResourceAssembly.GetName().Name);
            if (mutex.WaitOne(0, false))
            {
                try
                {
                    base.OnStartup(e);
                }
                catch (Exception ex)
                {
#if DEBUG
                    throw;
#else
                    ThrowException(ex);
                    Shutdown();
#endif
                }
            }
            else
            {
                MessageBox.Show("程序已经在运行！", "提示");
                Shutdown();
            }
            Debug.WriteLine("OnStartup");
        }


        void ThrowException(Exception e)
        {

            MessageBox.Show("我们很抱歉，当前应用程序遇到一些问题，该操作已经终止。我们将会上传错误日志以便开发人员解决问题。\n错误信息：" + e.Message, "意外的操作", MessageBoxButton.OK, MessageBoxImage.Information);
            Log.Error(e.ToString());
            Log.ReportError(e.ToString());
        }

        void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show("我们很抱歉，当前应用程序遇到一些问题，该操作已经终止。我们将会上传错误日志以便开发人员解决问题。\n错误信息：" + e.Exception.ToString(), "意外的操作", MessageBoxButton.OK, MessageBoxImage.Information);
            e.Handled = true;
            Log.Error(e.Exception.ToString());
            Log.ReportError(e.ToString());
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            MessageBox.Show("我们很抱歉，当前应用程序遇到一些问题，该操作已经终止。我们将会上传错误日志以便开发人员解决问题。\n错误信息：" + e.ToString(), "意外的操作", MessageBoxButton.OK, MessageBoxImage.Information);
            Log.Error(e.ToString());
            Log.ReportError(e.ToString());
        }

    }
}
