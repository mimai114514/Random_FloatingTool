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
using System.Windows.Threading;
using System.Reflection.Emit;

namespace Random_FloatingTool
{
    /// <summary>
    /// ToolBox.xaml 的交互逻辑
    /// </summary>
    public partial class ToolBox : Window
    {
        

        public string currectmode = "listmode";


        public string userFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        public string appFolder = "\\dev\\RandomUWP";
        public string listPath = "\\list.txt";
        public string logPath = "\\log.txt";

        public int numOfList;//列表数
        public int[] itemsInGroup = new int[110];//列表内项数
        public string[] nameOfGroup = new string[110];
        public string[,] item = new string[110, 1010];//内容列表

        public DispatcherTimer _flashTimer;

        public ToolBox()
        {
            InitializeComponent();
            InitializeTimer();

            if (!Directory.Exists(userFolder + appFolder))
            {
                Directory.CreateDirectory(userFolder + appFolder);
            }

            if (File.Exists(userFolder + appFolder + listPath))
            {
                StreamReader listReader = new(userFolder + appFolder + listPath);
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

            if(!File.Exists(userFolder+appFolder+logPath))
            {
                File.Create(userFolder + appFolder + logPath);
            }

            modeChange();
            
        }
        private void InitializeTimer()
        {
            _flashTimer = new DispatcherTimer();
            _flashTimer.Tick += FlashTimer_Tick;
            _flashTimer.Interval = TimeSpan.FromSeconds(0.02);
        }

        private void FlashTimer_Tick(object sender, EventArgs e)
        {
            Random random = new Random();
            if (currectmode == "nummode")
            {
                Result.Text = random.Next(Convert.ToInt16(nummode_min.Value), Convert.ToInt16(nummode_max.Value)).ToString();

            }
            else if (currectmode == "listmode")
            {
                Result.Text = item[listmode_combobox.SelectedIndex, random.Next(0, itemsInGroup[listmode_combobox.SelectedIndex])];
            }
        }

        private void RandomButton_Click(object sender, RoutedEventArgs e)
        {
            Random random = new Random();
            _flashTimer.Start();
            RandomButton.Visibility = Visibility.Hidden;
            StopButton.Visibility = Visibility.Visible;
            nummode_hide();
            listmode_hide();
            Result.Visibility = Visibility.Visible;
            Result_Side.Visibility = Visibility.Visible;
            Result_Side.Text = "被抽中的是..";
        }

        public void nummode_hide()
        {
            nummode_min.Visibility = Visibility.Hidden;
            nummode_max.Visibility = Visibility.Hidden;
            nummode_text_min.Visibility = Visibility.Hidden;
            nummode_text_max.Visibility = Visibility.Hidden;
            
        }

        public void nummode_show()
        {
            nummode_min.Visibility = Visibility.Visible;
            nummode_max.Visibility = Visibility.Visible;
            nummode_text_min.Visibility = Visibility.Visible;
            nummode_text_max.Visibility = Visibility.Visible;
            
        }

        public void listmode_show()
        {
            listmode_text.Visibility = Visibility.Visible;
            listmode_combobox.Visibility = Visibility.Visible;
            
        }

        public void listmode_hide()
        {
            listmode_text.Visibility = Visibility.Hidden;
            listmode_combobox.Visibility = Visibility.Hidden;
            
        }

        private void nummode(object sender, MouseButtonEventArgs e)
        {
            currectmode = "nummode";
            modeChange();
        }

        private void listmode(object sender, MouseButtonEventArgs e)
        {
            currectmode = "listmode";
            modeChange();
        }


        public void modeChange()
        {
            _flashTimer.Stop();
            Result.Visibility = Visibility.Hidden;
            Result_Side.Visibility = Visibility.Hidden;
            if (currectmode == "nummode")
            {
                nummode_show();
                listmode_hide();
                image_nummode.Source = new BitmapImage(new Uri("nummode_sel.png", UriKind.Relative));
                image_listmode.Source = new BitmapImage(new Uri("listmode.png", UriKind.Relative));
                currectmode = "nummode";
            }
            else if (currectmode == "listmode")
            {
                nummode_hide();
                listmode_show();
                image_nummode.Source = new BitmapImage(new Uri("nummode.png", UriKind.Relative));
                image_listmode.Source = new BitmapImage(new Uri("listmode_sel.png", UriKind.Relative));
                currectmode = "listmode";
            }
            RandomButton.Visibility = Visibility.Visible;
            StopButton.Visibility = Visibility.Hidden;
            FinishButton.Visibility = Visibility.Hidden;
        }

        private void close_button_Click(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
                Random_FloatingTool.App.Current.Shutdown();
            else
            {
                modeChange();
                this.Visibility= Visibility.Hidden;
            }
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            listmode_combobox.SelectedItem = listmode_combobox.Items[0];
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            _flashTimer.Stop();
            Result_Side.Text = "被抽中的是:";
            StreamWriter logWriter = new(userFolder + appFolder + logPath, true);
            logWriter.AutoFlush = true;
            logWriter.WriteLine(DateTime.Now.ToString() + " " +Result_Side.Text + Result.Text);
            logWriter.Close();
            StopButton.Visibility = Visibility.Hidden;
            FinishButton.Visibility= Visibility.Visible;
        }

        private void FinishButton_Click(object sender, RoutedEventArgs e)
        {
            FinishButton.Visibility = Visibility.Hidden;
            modeChange();
        }
    }
}
