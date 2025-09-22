using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Random_FloatingTool
{

    public partial class MainWindow : Window
    {
        //键盘热键相关
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const int HOTKEY_ID = 9000;
        private const uint MOD_CONTROL = 0x0002;
        private const uint R_KEY = 0x52;
        private const uint WM_HOTKEY = 0x0312;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            IntPtr handle = new WindowInteropHelper(this).Handle;
            bool success = RegisterHotKey(handle, HOTKEY_ID, MOD_CONTROL,R_KEY);

            if (!success)
            {
                MessageBox.Show("热键注册失败");
            }


            ComponentDispatcher.ThreadPreprocessMessage += (ref MSG msg, ref bool handled) =>
            {
                if (msg.message == WM_HOTKEY && (int)msg.wParam == HOTKEY_ID)
                {
                    ToggleToolBox();
                    toolBox.RandomButton.Focus();  //将焦点设为抽取按钮，方便键盘操作
                }
            };

            AutoStartManager.SetAutoStart(true);

            ToggleToolBox();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            IntPtr handle = new WindowInteropHelper(this).Handle;
            UnregisterHotKey(handle, HOTKEY_ID);
        }


        public ToolBox toolBox;
        public double startX, startY;
        public bool isWindowToggled = false;
        public double centerX,centerY;
        public MainWindow()
        {
            string processName = Process.GetCurrentProcess().ProcessName;
            if(Process.GetProcessesByName(processName).Length > 1)
            {
                Application.Current.Shutdown();
            }
            InitializeComponent();
            this.Opacity = 0.8;
            this.Left = 100;
            this.Top = SystemParameters.PrimaryScreenHeight - this.Height - 250;
            centerX = SystemParameters.PrimaryScreenWidth / 2;
            centerY = SystemParameters.PrimaryScreenHeight / 2;

        }

        public void ToggleToolBox()
        {
            if (toolBox == null)
            {
                toolBox = new ToolBox();
                toolBox.Owner = this;
                if (this.Left <= centerX)
                {
                    toolBox.Left = this.Left + this.Width + 20;
                }
                else
                {
                    toolBox.Left = this.Left - toolBox.Width - 20;
                }
                if(this.Top <= centerY)
                {
                    toolBox.Top = this.Top;
                }
                else
                {
                    toolBox.Top = this.Top - toolBox.Height + this.Height;
                }

                toolBox.Show();
                toolBox.Activate();
            }
            else
            {
                if(toolBox.Visibility == Visibility.Hidden)
                {
                    toolBox.Visibility = Visibility.Visible;
                    toolBox.Activate();
                    toolBox.modeChange();
                }
                else
                {
                    toolBox.modeChange();
                    toolBox.Visibility = Visibility.Hidden;

                }
            }
            isWindowToggled = !isWindowToggled;
        }

        private void Logo_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            startX = Window1.Left;
            startY = Window1.Top;
        }


        private void Logo_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if((Math.Abs(Window1.Left - startX) <20 && (Math.Abs(Window1.Top - startY) < 20))) //鼠标在任一方向均未移动超过20像素时，展开/折叠toolbox
            {
                ToggleToolBox();
            }
        }

        private void Logo_MouseMove(object sender, MouseEventArgs e)
        {
            
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void MainWindow_LocationChanged(object sender, EventArgs e)
        {
            if (toolBox != null)
            {

                if (this.Left <= centerX)
                {
                    toolBox.Left = this.Left + this.Width + 20;
                }
                else
                {
                    toolBox.Left = this.Left - toolBox.Width - 20;
                }
                if (this.Top <= centerY)
                {
                    toolBox.Top = this.Top;
                }
                else
                {
                    toolBox.Top = this.Top - toolBox.Height + this.Height;
                }
    public static class AutoStartManager
    {
        // 定义一个你的应用程序在注册表中的唯一名称
        private const string AppName = "Random_FloatingTool";

        /// <summary>
        /// 设置或取消开机自启动
        /// </summary>
        /// <param name="isEnabled">true为设置，false为取消</param>
        public static void SetAutoStart(bool isEnabled)
        {
            try
            {
                // 选择注册表根键
                RegistryKey baseKey = Registry.CurrentUser;

                // 打开 Run 键
                // 第二个参数 true 表示可写
                using (RegistryKey runKey = baseKey.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true))
                {
                    if (runKey == null)
                    {
                        MessageBox.Show("无法找到注册表 Run 键。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (isEnabled)
                    {
                        // 获取当前执行文件的完整路径
                        string executablePath = Assembly.GetEntryAssembly().Location;
                        // 设置键值。如果已存在同名键，会覆盖它
                        runKey.SetValue(AppName, executablePath);
                    }
                    else
                    {
                        // 如果键存在，则删除它
                        if (runKey.GetValue(AppName) != null)
                        {
                            runKey.DeleteValue(AppName, false);
                        }
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("权限不足，请以管理员身份运行程序来为所有用户设置自启动。", "权限错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"操作注册表时发生错误: {ex.Message}", "未知错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 检查当前是否已设置为自启动
        /// </summary>
        public static bool IsAutoStartEnabled()
        {
            RegistryKey baseKey = Registry.CurrentUser;
            using (RegistryKey runKey = baseKey.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", false))
            {
                return runKey?.GetValue(AppName) != null;
            }
        }
    }
}