using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Random_FloatingTool
{
    /// <summary>
    /// ToolBox.xaml 的交互逻辑
    /// </summary>
    public partial class ToolBox : Window
    {
        public ToolBox()
        {
            InitializeComponent();
        }

        private void RandomButton_Click(object sender, RoutedEventArgs e)
        {
            Random random = new Random();  
            Result.Text = random.Next(Convert.ToUInt16(Min.Text),Convert.ToUInt16(Max.Text)).ToString() + "被抽中了";
        }

        private void MinMinus_Click(object sender, RoutedEventArgs e)
        {
            Min.Text = (Convert.ToUInt16(Min.Text) - 1).ToString();
        }

        private void MinPlus_Click(object sender, RoutedEventArgs e)
        {
            Min.Text = (Convert.ToUInt16(Min.Text) + 1).ToString();
        }

        private void MaxMinus_Click(object sender, RoutedEventArgs e)
        {
            Max.Text = (Convert.ToUInt16(Max.Text) - 1).ToString();
        }

        private void MaxPlus_Click(object sender, RoutedEventArgs e)
        {
            Max.Text = (Convert.ToUInt16(Max.Text) + 1).ToString();
        }
    }
}
