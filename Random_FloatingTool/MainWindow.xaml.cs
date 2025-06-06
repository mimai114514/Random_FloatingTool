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
        private const uint MOD_ALT = 0x0001;
        private const uint R_KEY = 0x52;
        private const uint WM_HOTKEY = 0x0312;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            IntPtr handle = new WindowInteropHelper(this).Handle;
            bool success = RegisterHotKey(handle, HOTKEY_ID, MOD_ALT, R_KEY);

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

        }

        private void Window_Closed(object sender, EventArgs e)
        {
            IntPtr handle = new WindowInteropHelper(this).Handle;
            UnregisterHotKey(handle, HOTKEY_ID);
        }


        public ToolBox toolBox;
        public double startX, startY;
        public bool isWindowToggled = false;
        public double centerX, centerY;
        public MainWindow()
        {
            string processName = Process.GetCurrentProcess().ProcessName;
            if (Process.GetProcessesByName(processName).Length > 1)
            {
                Application.Current.Shutdown();
            }
            InitializeComponent();
            this.Opacity = 0.8;
            this.Left = 100;
            this.Top = SystemParameters.PrimaryScreenHeight - this.Height - 250;
            centerX = SystemParameters.PrimaryScreenWidth / 2;
            centerY = SystemParameters.PrimaryScreenHeight / 2;

            toolBox = new ToolBox();
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

            toolBox.Show();
            toolBox.Activate();
        }

        public void ToggleToolBox()
        {

            if (toolBox.Visibility == Visibility.Hidden)
            {
                toolBox.Visibility = Visibility.Visible;
                toolBox.Activate();
            }
            else
            {
                toolBox.modeChange();
                toolBox.Visibility = Visibility.Hidden;

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
            if ((Math.Abs(Window1.Left - startX) < 20 && (Math.Abs(Window1.Top - startY) < 20))) //鼠标在任一方向均未移动超过20像素时，展开/折叠toolbox
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
            }
        }
    }
}