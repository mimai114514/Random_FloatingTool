using System.Configuration;
using System.Data;
using System.Windows;

namespace Random_FloatingTool
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            // 捕获 UI 线程未处理异常
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            // 捕获非 UI 线程未处理异常
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            ShowException(e.Exception);
            e.Handled = true; // 尝试防止立即崩溃
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            ShowException(e.ExceptionObject as Exception);
        }

        private void ShowException(Exception ex)
        {
            if (ex == null) return;
            string msg = $"发生严重错误:\n{ex.Message}\n\n位置:\n{ex.StackTrace}";
            if (ex.InnerException != null)
            {
                msg += $"\n\n内部错误:\n{ex.InnerException.Message}\n{ex.InnerException.StackTrace}";
            }
            MessageBox.Show(msg, "程序启动崩溃诊断", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
