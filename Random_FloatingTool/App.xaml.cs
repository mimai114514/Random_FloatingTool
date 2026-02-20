using System.Configuration;
using System.Data;
using System.IO;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Random_FloatingTool
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private const string MutexName = "Random_FloatingTool_SingleInstance_Mutex";
        private const string PipeName = "Random_FloatingTool_Pipe";
        private Mutex _mutex;

        public App()
        {
            // 捕获 UI 线程未处理异常
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            // 捕获非 UI 线程未处理异常
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            const string mutexName = MutexName;
            bool createdNew;

            _mutex = new Mutex(true, mutexName, out createdNew);

            if (!createdNew)
            {
                // App is already running! Send message to existing instance.
                SendExpandCommandToExistingInstance();
                Shutdown();
                return;
            }

            // This is the first instance. Start the pipe server.
            Task.Run(() => StartPipeServer());

            base.OnStartup(e);

            // Manual MainWindow creation since StartupUri was removed
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
        }

        private async void StartPipeServer()
        {
            while (true)
            {
                try
                {
                    using (var server = new NamedPipeServerStream(PipeName, PipeDirection.In, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous))
                    {
                        await server.WaitForConnectionAsync();

                        using (var reader = new StreamReader(server))
                        {
                            var message = await reader.ReadToEndAsync();
                            if (message == "EXPAND")
                            {
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    var mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                                    if (mainWindow != null)
                                    {
                                        mainWindow.EnsureMainWindowVisible();
                                        mainWindow.ShowToolBox();
                                    }
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Handle or log error
                    System.Diagnostics.Debug.WriteLine($"Pipe Server Error: {ex.Message}");
                    // Wait a bit before restarting loop to avoid tight loop on persistent error
                    await Task.Delay(1000); 
                }
            }
        }

        private void SendExpandCommandToExistingInstance()
        {
            try
            {
                using (var client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out))
                {
                    client.Connect(1000); // Wait 1 second for connection
                    using (var writer = new StreamWriter(client))
                    {
                        writer.Write("EXPAND");
                        writer.Flush();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not connect to existing instance: {ex.Message}");
            }
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
