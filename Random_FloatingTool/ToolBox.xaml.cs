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
using System.Timers;

namespace Random_FloatingTool
{
    public partial class ToolBox : Window
    {

        public string currectmode = "listmode";

        public string userFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        public string appFolder = "\\dev\\Random";
        public string listPath = "\\list.txt";
        public string logPath = "\\log.txt";

        public int listCount;//列表数
        public int[] itemCount = new int[110];//列表内项数
        public string[] nameOfGroup = new string[110];//列表名
        public string[,] item = new string[110, 1010];//内容列表

        public DispatcherTimer _flashTimer;//抽取项更新计时器
        public DispatcherTimer _autoToggleTimer;//自动折叠计时器
        public StreamWriter logWriter;

        public ToolBox()
        {
            InitializeComponent();
            InitializeTimer();
            InitializeLogWriter();

            if (!Directory.Exists(userFolder + appFolder))
            {
                Directory.CreateDirectory(userFolder + appFolder);
            }

            if (File.Exists(userFolder + appFolder + listPath))
            {
                try
                {
                    StreamReader listReader = new(userFolder + appFolder + listPath);
                    listCount = Convert.ToInt16(listReader.ReadLine());//读取列表数
                    int groupCount;
                    for (groupCount = 0; groupCount < listCount; groupCount++)//读取每个列表的名称、项数和内容
                    {
                        int numOfItem;
                        nameOfGroup[groupCount] = listReader.ReadLine();
                        numOfItem = Convert.ToInt16(listReader.ReadLine());
                        itemCount[groupCount] = numOfItem;
                        listmode_combobox.Items.Add(nameOfGroup[groupCount]);
                        int itemReadingCount;
                        for (itemReadingCount = 0; itemReadingCount < numOfItem; itemReadingCount++)
                        {
                            item[groupCount, itemReadingCount] = listReader.ReadLine();
                        }
                    }
                    listReader.Close();
                } 
                catch
                {
                    listmode_combobox.Items[0]="列表读取出错";
                    listmode_combobox.IsEnabled = false;
                    RandomButton.IsEnabled = false;
                }
            }
            else
            {
                listmode_combobox.Items[0]="无列表文件";
                listmode_combobox.IsEnabled = false;
                RandomButton.IsEnabled = false;
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
            _autoToggleTimer = new DispatcherTimer();
            _autoToggleTimer.Tick += AutoToggle;
            _autoToggleTimer.Interval = TimeSpan.FromSeconds(12.5);
        }

        public void InitializeLogWriter()
        {
            logWriter = new(userFolder + appFolder + logPath, true);
            logWriter.AutoFlush = true;
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
                Result.Text = item[listmode_combobox.SelectedIndex, random.Next(0, itemCount[listmode_combobox.SelectedIndex])];
            }
        }

        private void AutoToggle(object sender, EventArgs e)
        {
            _autoToggleTimer.Stop();
            if (this.IsMouseOver)
            {
                _autoToggleTimer.Start();
            }
            else
            {
                this.Visibility = Visibility.Hidden;
                modeChange();
            }
        }

        private void RandomButton_Click(object sender, RoutedEventArgs e)
        {
            _autoToggleTimer.Stop();
            Random random = new Random();
            if(nummode_min.Value>nummode_max.Value)
            {
                double? tmp = nummode_min.Value;
                nummode_min.Value = nummode_max.Value;
                nummode_max.Value = tmp;
            }
            _flashTimer.Start();
            RandomButton.Visibility = Visibility.Hidden;
            StopButton.Visibility = Visibility.Visible;
            StopButton.Focus();
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

        public void modeChange()
        {
            _flashTimer.Stop();
            _autoToggleTimer.Start();
            Result.Visibility = Visibility.Hidden;
            Result_Side.Visibility = Visibility.Hidden;
            if (currectmode == "nummode")
            {
                nummode_show();
                listmode_hide();
                currectmode = "nummode";
            }
            else if (currectmode == "listmode")
            {
                nummode_hide();
                listmode_show();
                currectmode = "listmode";
            }
            RandomButton.Visibility = Visibility.Visible;
            RandomButton.Focus();
            StopButton.Visibility = Visibility.Hidden;
            FinishButton.Visibility = Visibility.Hidden;
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            listmode_combobox.SelectedItem = listmode_combobox.Items[0];
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            _flashTimer.Stop();
            _autoToggleTimer.Start();
            Result_Side.Text = "被抽中的是:";
            logWriter.WriteLine(DateTime.Now.ToString() + " 被抽中的是:" + Result.Text);
            StopButton.Visibility = Visibility.Hidden;
            FinishButton.Visibility= Visibility.Visible;
            FinishButton.Focus();
        }

        private void FinishButton_Click(object sender, RoutedEventArgs e)
        {
            FinishButton.Visibility = Visibility.Hidden;
            _autoToggleTimer.Start();
            modeChange();
        }

        private void nummode_button_Click(object sender, RoutedEventArgs e)
        {
            currectmode = "nummode";
            modeChange();
        }

        private void listmode_button_Click(object sender, RoutedEventArgs e)
        {
            currectmode = "listmode";
            modeChange();
        }


        private void close_button_Click(object sender, RoutedEventArgs e)
        {
            _autoToggleTimer.Stop();
            modeChange();
            this.Visibility = Visibility.Hidden;
        }


        private void close_button_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            _autoToggleTimer.Stop();
            _flashTimer.Stop();
            logWriter.Flush();
            logWriter.Close();
            Application.Current.Shutdown();
        }
    }
}
