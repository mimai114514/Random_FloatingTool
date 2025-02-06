using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
using System.Threading.Tasks;

namespace Random_FloatingTool
{
    /// <summary>
    /// ToolBox.xaml 的交互逻辑
    /// </summary>
    public partial class ToolBox : Window
    {
        public string currectmode = "nummode";

        public string folderPath = "C:\\Users\\Infinity\\Documents\\dev\\RandomUWP";
        public string listPath = "C:\\Users\\Infinity\\Documents\\dev\\RandomUWP\\list.txt";

        public int numOfList;//列表数
        public int[] itemsInGroup = new int[110];//列表内项数
        public string[] nameOfGroup = new string[110];
        public string[,] item = new string[110, 1010];//内容列表

        public ToolBox()
        {
            InitializeComponent();

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            if (File.Exists(listPath))
            {
                StreamReader listReader = new(listPath);
                numOfList = Convert.ToInt16(listReader.ReadLine());//读取列表数
                int groupCount;
                for (groupCount = 0; groupCount < numOfList; groupCount++)
                {
                    int numOfItem;
                    nameOfGroup[groupCount] = listReader.ReadLine();
                    numOfItem = Convert.ToInt16(listReader.ReadLine());
                    itemsInGroup[groupCount] = numOfItem;
                    listmode_combobox.Items.Add(nameOfGroup[groupCount]);
                    int itemReadingCount;
                    for (itemReadingCount = 0; itemReadingCount < numOfItem; itemReadingCount++)
                    {
                        item[groupCount, itemReadingCount] = listReader.ReadLine();
                    }
                }
            }
            else
            {
                listmode_combobox.Items.Add("无列表文件");
            }

            
        }

        private void RandomButton_Click(object sender, RoutedEventArgs e)
        {
            Random random = new Random();
            if (currectmode == "nummode")
                Result.Text = random.Next(Convert.ToInt16(nummode_number_min.Text), Convert.ToInt16(nummode_number_max.Text)).ToString() + " 被抽中了";
            else if (currectmode == "listmode")
            {
                Result.Text = item[listmode_combobox.SelectedIndex, random.Next(0, itemsInGroup[listmode_combobox.SelectedIndex])] + " 被抽中了";
            }
        }

        private void min_minus_left(object sender, RoutedEventArgs e)
        {
            nummode_number_min.Text = (Convert.ToInt16(nummode_number_min.Text) - 1).ToString();
        }

        private void min_minus_right(object sender, MouseButtonEventArgs e)
        {
            nummode_number_min.Text = (Convert.ToInt16(nummode_number_min.Text) - 10).ToString();
        }

        private void min_plus_left(object sender, RoutedEventArgs e)
        {
            nummode_number_min.Text = (Convert.ToInt16(nummode_number_min.Text) + 1).ToString();
        }

        private void min_plus_right(object sender, MouseButtonEventArgs e)
        {
            nummode_number_min.Text = (Convert.ToInt16(nummode_number_min.Text) + 10).ToString();
        }

        private void max_minus_left(object sender, RoutedEventArgs e)
        {
            nummode_number_max.Text = (Convert.ToInt16(nummode_number_max.Text) - 1).ToString();
        }

        private void max_minus_right(object sender, MouseButtonEventArgs e)
        {
            nummode_number_max.Text = (Convert.ToInt16(nummode_number_max.Text) - 10).ToString();
        }

        private void max_plus_left(object sender, RoutedEventArgs e)
        {
            nummode_number_max.Text = (Convert.ToInt16(nummode_number_max.Text) + 1).ToString();
        }

        private void max_plus_right(object sender, MouseButtonEventArgs e)
        {
            nummode_number_max.Text = (Convert.ToInt16(nummode_number_max.Text) + 10).ToString();
        }


        public void nummode_hide()
        {
            nummode_number_min.Visibility = Visibility.Hidden;
            nummode_number_max.Visibility = Visibility.Hidden;
            nummode_button_min_minus.Visibility = Visibility.Hidden;
            nummode_button_min_plus.Visibility = Visibility.Hidden;
            nummode_button_max_minus.Visibility = Visibility.Hidden;
            nummode_button_max_plus.Visibility = Visibility.Hidden;
            nummode_text_min.Visibility = Visibility.Hidden;
            nummode_text_max.Visibility = Visibility.Hidden;
            image_nummode.Source = new BitmapImage(new Uri("nummode.png", UriKind.Relative));
        }

        public void nummode_show()
        {
            nummode_number_min.Visibility = Visibility.Visible;
            nummode_number_max.Visibility = Visibility.Visible;
            nummode_button_min_minus.Visibility = Visibility.Visible;
            nummode_button_min_plus.Visibility = Visibility.Visible;
            nummode_button_max_minus.Visibility = Visibility.Visible;
            nummode_button_max_plus.Visibility = Visibility.Visible;
            nummode_text_min.Visibility = Visibility.Visible;
            nummode_text_max.Visibility = Visibility.Visible;
            image_nummode.Source = new BitmapImage(new Uri("nummode_sel.png", UriKind.Relative));
        }

        public void listmode_show()
        {
            listmode_text.Visibility = Visibility.Visible;
            listmode_combobox.Visibility = Visibility.Visible;
            image_listmode.Source = new BitmapImage(new Uri("listmode_sel.png", UriKind.Relative));
        }

        public void listmode_hide()
        {
            listmode_text.Visibility = Visibility.Hidden;
            listmode_combobox.Visibility = Visibility.Hidden;
            image_listmode.Source = new BitmapImage(new Uri("listmode.png", UriKind.Relative));
        }

        private void nummode(object sender, MouseButtonEventArgs e)
        {
            nummode_show();
            listmode_hide();
            currectmode = "nummode";
        }

        private void listmode(object sender, MouseButtonEventArgs e)
        {
            nummode_hide();
            listmode_show();
            currectmode = "listmode";
        }

        private void close_app(object sender, MouseButtonEventArgs e)
        {
            Random_FloatingTool.App.Current.Shutdown();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            listmode_combobox.SelectedItem = listmode_combobox.Items[0];
        }

    }
}
