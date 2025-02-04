using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Random_FloatingTool
{

    public partial class MainWindow : Window
    {
        public ToolBox toolBox;
        public double startX, startY;
        public bool isWindowToggled = false;
        public double centerX;
        public MainWindow()
        {
            InitializeComponent();
            this.Opacity = 0.8;

            // 获取屏幕宽度
            centerX = SystemParameters.PrimaryScreenWidth / 2;
        }

        private void ToggleToolBox()
        {
            if (toolBox == null)
            {
                toolBox = new ToolBox();
                toolBox.Owner = this;
                if (this.Left <= centerX)
                {
                    toolBox.Left = this.Left + this.Width + 20;
                    toolBox.Top = this.Top;
                }
                else
                {
                    toolBox.Left = this.Left - toolBox.Width - 20;
                    toolBox.Top = this.Top;
                }
                toolBox.Show();
            }
            else
            {
                toolBox.Visibility = toolBox.IsVisible
                    ? Visibility.Hidden
                    : Visibility.Visible;
            }
            isWindowToggled = !isWindowToggled;
        }

        private void Logo_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            startX = Window1.Left;
            startY = Window1.Top;
            ToggleToolBox();
        }

        private void Logo_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
                if ((Math.Abs(Window1.Left - startX) > 10 || (Math.Abs(Window1.Top - startY) > 10)) && toolBox != null)
                {
                    ToggleToolBox();
                }
            }
        }

        private void MainWindow_LocationChanged(object sender, EventArgs e)
        {
            if (toolBox != null)
            {

                if (this.Left <= centerX)
                {
                    toolBox.Left = this.Left + this.Width + 20;
                    toolBox.Top = this.Top;
                }
                else
                {
                    toolBox.Left = this.Left - toolBox.Width - 20;
                    toolBox.Top = this.Top;
                }
            }
        }
    }
}