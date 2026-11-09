using HandyControl.Themes;
using ImageManager.Logging;
using ImageManager.Tools;
using ImageManager.Windows;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using LoggerFactory = ImageManager.Logging.LoggerFactory;
using LogLevel = ImageManager.Logging.LogLevel;

namespace ImageManager
{
    public partial class App : Application
    {
        private Logger _logger = LoggerFactory.GetLogger(nameof(App));
        public static Version AppVersion =>
            Assembly.GetEntryAssembly()!.GetName().Version!;

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
            var location = Assembly.GetEntryAssembly()!.Location;
            var bs = MD5.HashData(Encoding.UTF8.GetBytes(location));
            mutex = new Mutex(true, Convert.ToBase64String(bs), out var isNewInstance);
            if (isNewInstance)
            {
                try
                {
                    var cpuId = Machine.GetCpuId();
                    _logger.Log(LogLevel.Info, $"CPU ID: {cpuId}");
                    var boardId = Machine.GetBoardId();
                    _logger.Log(LogLevel.Info, $"Board ID: {boardId}");
                    var hdId = Machine.GetHDId();
                    _logger.Log(LogLevel.Info, $"HD ID: {hdId}");
                    var osName = Machine.GetOperatingSystem();
                    _logger.Log(LogLevel.Info, $"OS: {osName} ({RuntimeInformation.OSArchitecture})");
                    var machineCode = Machine.GetMachineCode();
                    _logger.Log(LogLevel.Info, $"Machine Code: {machineCode}");

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
            // 关闭截图窗口
            ScreenShotWindow.Instance?.Close();
            // 之后紧接着 Shutdown，须同步上报
            LoggerFactory.GetLogger(nameof(App)).Log(LogLevel.Fatal, exception: e, report: true, waitReport: true);
            MessageBox.Show("我们很抱歉，当前应用程序遇到一些问题，该操作已经终止。我们将会上传错误日志以便开发人员解决问题。\n错误信息：" +
                e.Message, "意外的操作", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // 关闭截图窗口
            ScreenShotWindow.Instance?.Close();
            LoggerFactory.GetLogger(nameof(App)).Log(LogLevel.Fatal, exception: e.Exception, report: true);
            MessageBox.Show("我们很抱歉，当前应用程序遇到一些问题，该操作已经终止。我们将会上传错误日志以便开发人员解决问题。\n错误信息：" +
                e.Exception.Message, "意外的操作", MessageBoxButton.OK, MessageBoxImage.Information);
            e.Handled = true;
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            // 关闭截图窗口
            ScreenShotWindow.Instance?.Close();
            var ex = (Exception)e.ExceptionObject;
            // 进程即将退出，须同步上报
            LoggerFactory.GetLogger(nameof(App)).Log(LogLevel.Fatal, exception: ex, report: true, waitReport: true);
            MessageBox.Show("我们很抱歉，当前应用程序遇到一些问题，该操作已经终止。我们将会上传错误日志以便开发人员解决问题。\n错误信息：" +
                ex.Message, "意外的操作", MessageBoxButton.OK, MessageBoxImage.Information);
        }

    }
}
