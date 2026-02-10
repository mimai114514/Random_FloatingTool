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
using System.Security.RightsManagement;

namespace Random_FloatingTool
{
    /// <summary>
    /// ToolBox.xaml 的交互逻辑
    /// </summary>
    public partial class ToolBox : Window
    {

        public string currectmode = "listmode";
        public bool isAnyListExist = true;
        public bool isDedupeOn = false;
        public List<(int Id, string Content)> currentList = new List<(int, string)>();

        public string userFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        public string appFolder = "\\dev\\Random";
        public string dbFileName = "random.db";

        private DatabaseService _db;

        public int numOfList = 0;//列表数
        public List<(int Id, string Name)> listGroups = new List<(int, string)>();//列表组（含ID）
        public List<List<(int Id, string Content)>> listItems = new List<List<(int, string)>>();//列表内容（含ID）

        public DispatcherTimer _flashTimer;
        public DispatcherTimer _autoToggleTimer;

        public double screenCenterX, screenCenterY, screenHeight, screenWidth;

        private MainWindow _mainWindow;

        public ToolBox(MainWindow mainWindow)
        {
            InitializeComponent();
            InitializeTimer();

            screenCenterX = SystemParameters.PrimaryScreenWidth / 2;
            screenCenterY = SystemParameters.PrimaryScreenHeight / 2;
            screenHeight = SystemParameters.PrimaryScreenHeight;
            screenWidth = SystemParameters.PrimaryScreenWidth;

            // 初始化数据库
            string dbPath = Path.Combine(userFolder, appFolder.TrimStart('\\'), dbFileName);
            _db = new DatabaseService(dbPath);

            // 从数据库加载列表
            try
            {
                listGroups = _db.GetAllGroups();
                numOfList = listGroups.Count;

                if (numOfList > 0)
                {
                    foreach (var group in listGroups)
                    {
                        listmode_combobox.Items.Add(group.Name);
                        listItems.Add(_db.GetItemsByGroup(group.Id));
                    }
                }
                else
                {
                    listmode_combobox.Items.Add("无列表");
                    isAnyListExist = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("数据库读取错误：\n" + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                listmode_combobox.Items.Add("无列表");
                numOfList = 0;
                isAnyListExist = false;
            }

            if (!isAnyListExist)
                currectmode = "nummode";

            _mainWindow = mainWindow;
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

        private void FlashTimer_Tick(object sender, EventArgs e)
        {
            Random random = new Random();
            if (currectmode == "nummode")
            {
                Result.Text = random.Next(Convert.ToInt16(nummode_min.Value), Convert.ToInt16(nummode_max.Value)).ToString();

            }
            else if (currectmode == "listmode")
            {
                if (listmode_combobox.SelectedIndex >= 0)
                {
                    if (currentList.Count > 0)
                    {
                        var item = currentList[random.Next(0, currentList.Count)];
                        Result.Text = item.Content;
                    }
                    else
                    {
                        Result.Text = "列表为空";
                    }
                }
                else
                {
                    Result.Text = "无效列表";
                }
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
            _flashTimer.Start();
            RandomButton.Visibility = Visibility.Hidden;
            StopButton.Visibility = Visibility.Visible;
            StopButton.Focus();
            nummode_hide();
            listmode_hide();
            Result.Visibility = Visibility.Visible;
            Result_Side.Visibility = Visibility.Visible;
            Result_Side.Text = "被抽中的是...";
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
            listmode_text_dedupe.Visibility= Visibility.Visible;
            listmode_dedupe_switch.Visibility= Visibility.Visible;
            listmode_item_count_text.Visibility= Visibility.Visible;
        }

        public void listmode_hide()
        {
            listmode_text.Visibility = Visibility.Hidden;
            listmode_combobox.Visibility = Visibility.Hidden;
            listmode_text_dedupe.Visibility = Visibility.Hidden;
            listmode_dedupe_switch.Visibility = Visibility.Hidden;
            listmode_item_count_text.Visibility = Visibility.Hidden;

        }

        public void modeChange()
        {
            _flashTimer.Stop();
            Result.Visibility = Visibility.Hidden;
            Result_Side.Visibility = Visibility.Hidden;
            if (currectmode == "nummode")
            {
                nummode_button.IsEnabled = false;
                nummode_button.Foreground= Brushes.DarkGray;
                mode_icon_num.Visibility= Visibility.Visible;
                mode_icon_list.Visibility=Visibility.Collapsed;
                mode_text.Text = "数字模式";
                listmode_button.IsEnabled = true;
                listmode_button.Foreground = Brushes.White;
                nummode_show();
                listmode_hide();
                RandomButton.IsEnabled = true;
                currectmode = "nummode";
            }
            else if (currectmode == "listmode")
            {
                listmode_button.IsEnabled = false;
                listmode_button.Foreground = Brushes.DarkGray;
                mode_icon_num.Visibility = Visibility.Collapsed;
                mode_icon_list.Visibility = Visibility.Visible;
                mode_text.Text = "列表模式";
                nummode_button.IsEnabled = true;
                nummode_button.Foreground = Brushes.White;
                nummode_hide();
                listmode_show();
                if (isAnyListExist)
                {
                    RandomButton.IsEnabled=true;
                }
                else
                {
                    RandomButton.IsEnabled = false;
                }
                currectmode = "listmode";
            }

            _mainWindow.Broadcast("mode:" + currectmode);//remote

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

            // 写入数据库日志
            try
            {
                if (currectmode == "nummode")
                {
                    if (int.TryParse(Result.Text, out int num))
                    {
                        _db.AddNumModeLog(num);
                    }
                }
                else if (currectmode == "listmode" && listmode_combobox.SelectedIndex >= 0)
                {
                    int groupId = listGroups[listmode_combobox.SelectedIndex].Id;
                    // 查找抽中项的 ID
                    var drawnItem = currentList.Find(x => x.Content == Result.Text);
                    if (drawnItem.Id > 0)
                    {
                        _db.AddListModeLog(groupId, drawnItem.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"日志写入错误: {ex.Message}");
            }

            // 去重逻辑
            if (currectmode == "listmode" && isDedupeOn)
            {
                var matchItem = currentList.Find(x => x.Content == Result.Text);
                if (matchItem.Id > 0)
                {
                    if (currentList.Count > 1)
                    {
                        currentList.Remove(matchItem);
                        updateItemCountText();
                    }
                    else
                    {
                        currentList = new List<(int Id, string Content)>(listItems[listmode_combobox.SelectedIndex]);
                        updateItemCountText();
                    }
                }
            }
            StopButton.Visibility = Visibility.Hidden;
            FinishButton.Visibility = Visibility.Visible;
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
            this.Visibility = Visibility.Hidden;
        }


        private void close_button_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void listmode_dedupe_switch_Click(object sender, RoutedEventArgs e)
        {
            if(listmode_dedupe_switch.IsChecked == true)
            {
                isDedupeOn = true;
                currentList = new List<(int Id, string Content)>(listItems[listmode_combobox.SelectedIndex]);
                updateItemCountText();
            }
            else
            {
                isDedupeOn = false;
                currentList = new List<(int Id, string Content)>(listItems[listmode_combobox.SelectedIndex]);
                updateItemCountText();
            }
        }

        private void listmode_combobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            currentList = new List<(int Id, string Content)>(listItems[listmode_combobox.SelectedIndex]);
            updateItemCountText();
        }

        private void updateItemCountText()
        {
            if(isDedupeOn)
            {
                listmode_item_count_text.Text = "还剩"+currentList.Count.ToString()+"项";
            }
            else
            {
                listmode_item_count_text.Text = "共" + currentList.Count.ToString() + "项";
            }
        }

        private void nummode_button_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            currectmode = "nummode";
            modeChange();
        }

        private void ToolBar_LocationChanged(object sender, EventArgs e)
        {
            //relocateMainWindow();
        }

        private void listmode_button_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            currectmode="listmode";
            modeChange();
        }

        private void ToolBar_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
                relocateMainWindow();
            }
        }

        public void relocateMainWindow()
        {
            //listmode_item_count_text.Text = (this.Left + this.Width / 2).ToString() + " " + screenCenterX.ToString();
            //listmode_item_count_text.Text = this.WindowState.ToString();
            double targetX, targetY;

            if (this.Left+this.Width/2<=screenCenterX)
            {
                targetX = this.Left - _mainWindow.Width - 20 >= 0 ? this.Left - _mainWindow.Width - 20 : 0;
            }
            else
            {
                targetX = this.Left + this.Width + 20 <= screenWidth ? this.Left + this.Width + 20 : screenWidth - _mainWindow.Width;
            }
            

            //listmode_item_count_text.Text = (this.Top + this.Height / 2).ToString();// +" "+ screenCenterY.ToString();
            
            if (this.Top+this.Height/2<= screenCenterY)
            {
                targetY = this.Top;
            }
            else
            {
                targetY=this.Top+this.Height-_mainWindow.Height;
            }

            _mainWindow.Left = targetX;
            _mainWindow.Top = targetY;
        }
    }
}
