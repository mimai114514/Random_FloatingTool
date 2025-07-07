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
using System.Data.SQLite;
using System.Data.Entity.Core.Common;
using System.Globalization;

namespace Random_FloatingTool
{
    public class DatabaseHelper
    {
        public string userFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        public string appFolder = "\\Random";
        public string dbFile = "\\random.db";
        public string appFolderPath => userFolder + appFolder;

        public string databasePath => userFolder + appFolder + dbFile;

        public DatabaseHelper()
        {

        }



        //初始化数据库和表结构
        public void InitializeDatabase()
        {
            if (!Directory.Exists(appFolderPath))
            {
                Directory.CreateDirectory(appFolderPath);
            }

            if (!File.Exists(databasePath))
            {
                SQLiteConnection.CreateFile(databasePath);
            }

            using (var connection = new SQLiteConnection($"Data Source={databasePath};Version=3;"))
            {
                connection.Open();

                // 创建列表表
                const string createListTable = @"
                CREATE TABLE IF NOT EXISTS Lists (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    UsageCount INTEGER DEFAULT 0,
                    WeightSum INTEGER
                );";

                // 创建项表
                const string createItemsTable = @"
                CREATE TABLE IF NOT EXISTS Items (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    ListId INTEGER NOT NULL,
                    Name TEXT NOT NULL,
                    UsageCount INTEGER DEFAULT 0,
                    Weight INTEGER DEFAULT 100,
                    FOREIGN KEY(ListId) REFERENCES Lists(Id)
                );";

                //创建日志表
                const string createLogTable = @"
                CREATE TABLE IF NOT EXISTS Logs (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Time TEXT,
                    ListId INTEGER NOT NULL,
                    ItemId INTEGER NOT NULL
                );";

                using (var command = new SQLiteCommand(createListTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                using (var command = new SQLiteCommand(createItemsTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                using (var command = new SQLiteCommand(createLogTable, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        //读取所有列表信息
        public List<ListInfo> GetLists()
        {
            var lists = new List<ListInfo>();

            using (var connection = new SQLiteConnection($"Data Source={databasePath};Version=3;"))
            {
                connection.Open();
                const string query = "SELECT Id, Name, UsageCount, WeightSum FROM Lists";

                using (var command = new SQLiteCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lists.Add(new ListInfo
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            UsageCount = reader.GetInt32(2),
                            WeightSum = reader.IsDBNull(3) ? 0 : reader.GetInt32(3)
                        });
                    }
                }
            }
            return lists;
        }

        public string GetListName(int ListId)
        {
            string listName;

            using (var connection = new SQLiteConnection($"Data Source={databasePath};Version=3;"))
            {
                connection.Open();
                const string query = "SELECT Name FROM Lists WHERE Id = @ListId";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ListId", ListId);

                    using (var reader = command.ExecuteReader())
                    {
                        reader.Read();
                        listName = reader.GetString(0);
                    }
                }
            }
            return listName;
        }

        //根据列表ID获取项
        public List<Item> GetItemsByListId(int listId)
        {
            var items = new List<Item>();

            using (var connection = new SQLiteConnection($"Data Source={databasePath};Version=3;"))
            {
                connection.Open();
                const string query = "SELECT Id, Name, UsageCount, Weight FROM Items WHERE ListId = @ListId";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ListId", listId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            items.Add(new Item
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                UsageCount = reader.GetInt32(2),
                                Weight = reader.GetInt32(3),
                                ListId = listId
                            });
                        }
                    }
                }
            }
            return items;
        }

        public Item GetItemById(int ItemId)
        {
            var item = new Item();
            using (var connection = new SQLiteConnection($"Data Source={databasePath};Version=3;"))
            {
                connection.Open();
                const string query = "SELECT Id, Name, UsageCount, Weight, ListId FROM Items WHERE Id = @Id";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", ItemId);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            item.Id = reader.GetInt32(0);
                            item.Name = reader.GetString(1);
                            item.UsageCount = reader.GetInt32(2);
                            item.Weight = reader.GetInt32(3);
                            item.ListId = reader.GetInt32(4);
                        }
                    }
                }
            }
            return item;
        }

        public void AddItem(int listId, string itemName)
        {
            using (var connection = new SQLiteConnection($"Data Source={databasePath};Version=3;"))
            {
                connection.Open();
                const string query = @"
                INSERT INTO Items (ListId, Name, UsageCount) 
                VALUES (@ListId, @Name, 0)";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ListId", listId);
                    command.Parameters.AddWithValue("@Name", itemName);
                    command.ExecuteNonQuery();
                }
            }
        }

        public int AddList(string listName)
        {
            using (var connection = new SQLiteConnection($"Data Source={databasePath};Version=3;"))
            {
                connection.Open();
                const string query = @"
                INSERT INTO Lists (Name, UsageCount) 
                VALUES (@Name, 0)";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Name", listName);
                    command.ExecuteNonQuery();
                    return (int)connection.LastInsertRowId;
                }
            }
        }

        public void RenameList(int listId, string newName)
        {
            using (var connection = new SQLiteConnection($"Data Source={databasePath};Version=3;"))
            {
                connection.Open();
                const string query = @"
                UPDATE Lists 
                SET Name = @Name 
                WHERE Id = @Id";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", listId);
                    command.Parameters.AddWithValue("@Name", newName);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void UpdateListWeightSum(int ListId)
        {
            List<Item> itemsOfListToUpdate = GetItemsByListId(ListId);
            int WeightSum = 0;
            foreach (Item item in itemsOfListToUpdate)
            {
                WeightSum += item.Weight;
            }
            using (var connection = new SQLiteConnection($"Data Source={databasePath};Version=3;"))
            {
                connection.Open();
                const string query = @"
                UPDATE Lists 
                SET WeightSum = @WeightSum 
                WHERE Id = @Id";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", ListId);
                    command.Parameters.AddWithValue("@WeightSum", WeightSum);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void DeleteList(int listId)
        {
            using (var connection = new SQLiteConnection($"Data Source={databasePath};Version=3;"))
            {
                connection.Open();
                string query = "DELETE FROM Lists WHERE Id = @Id";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", listId);
                    command.ExecuteNonQuery();
                }
                query = "DELETE FROM Items WHERE ListId = @ListId";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ListId", listId);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void DeleteItem(int ItemID)
        {
            using (var connection = new SQLiteConnection($"Data Source={databasePath};Version=3;"))
            {
                connection.Open();
                const string query = "DELETE FROM Items WHERE Id = @Id";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", ItemID);
                    command.ExecuteNonQuery();
                }
            }
        }

        //更新项信息
        public int UpdateItem(Item item)
        {
            using (var connection = new SQLiteConnection($"Data Source={databasePath};Version=3;"))
            {
                connection.Open();
                const string query = @"
                UPDATE Items 
                SET Name = @Name, Weight = @Weight 
                WHERE Id = @Id";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", item.Id);
                    command.Parameters.AddWithValue("@Name", item.Name);
                    command.Parameters.AddWithValue("@Weight", item.Weight);

                    return command.ExecuteNonQuery();
                }
            }
        }

        public void IncreaseUsageCount(int itemID)
        {
            using (var connection = new SQLiteConnection($"Data Source={databasePath};Version=3;"))
            {
                connection.Open();
                const string query_item = @"
                UPDATE Items 
                SET UsageCount = UsageCount + 1 
                WHERE Id = @Id";
                using (var command = new SQLiteCommand(query_item, connection))
                {
                    command.Parameters.AddWithValue("@Id", itemID);
                    command.ExecuteNonQuery();
                }
                const string getListIdQuery = @"
                SELECT ListId FROM Items WHERE Id = @Id";
                using (var command = new SQLiteCommand(getListIdQuery, connection))
                {
                    command.Parameters.AddWithValue("@Id", itemID);
                    int listId = Convert.ToInt32(command.ExecuteScalar());
                    const string query_list = @"
                    UPDATE Lists 
                    SET UsageCount = UsageCount + 1 
                    WHERE Id = @ListId";
                    using (var listCommand = new SQLiteCommand(query_list, connection))
                    {
                        listCommand.Parameters.AddWithValue("@ListId", listId);
                        listCommand.ExecuteNonQuery();
                    }
                }
            }
        }

        public void WriteLog(DateTime time, int listId, int itemId)
        {
            using (var connection = new SQLiteConnection($"Data Source={databasePath};Version=3;"))
            {
                connection.Open();
                const string query = @"
                INSERT INTO Logs (Time, ListId, ItemId) 
                VALUES (@Time, @ListId, @ItemId)";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Time", TimeHelper.ToLocalTimeString(time));
                    command.Parameters.AddWithValue("@ListId", listId);
                    command.Parameters.AddWithValue("@ItemId", itemId);
                    command.ExecuteNonQuery();
                }
            }
        }

    }

    public static class TimeHelper
    {
        // 本地时间 → 数据库字符串
        public static string ToLocalTimeString(this DateTime time)
        {
            return time.ToString("yyyy-MM-dd HH:mm:ss");
        }

        // 数据库字符串 → 本地时间
        public static DateTime ParseLocalTime(string dbTimeString)
        {
            return DateTime.ParseExact(
                dbTimeString,
                "yyyy-MM-dd HH:mm:ss",
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeLocal
            );
        }
    }

    // 数据模型类
    public class ListInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int UsageCount { get; set; }
        public int WeightSum { get; set; }


    }

    public class Item
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int UsageCount { get; set; }
        public int Weight { get; set; }
        public int ListId { get; set; }
    }
    public partial class ToolBox : Window
    {

        public string currectmode = "listmode";
        /*
        public string userFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        public string appFolder = "\\dev\\Random";
        public string listPath = "\\list.txt";
        public string logPath = "\\log.txt";
        */
        public int selectedItemIndex;

        public DispatcherTimer _flashTimer;//抽取项更新计时器
        public DispatcherTimer _autoToggleTimer;//自动折叠计时器
        //public StreamWriter logWriter;

        public DatabaseHelper db;
        public List<ListInfo> lists;
        public List<Item> currentList = new List<Item>();
        public int weightSum;
        public Item[] currentListWithWeight;

        public ToolBox()
        {
            InitializeComponent();
            InitializeTimer();
            //InitializeLogWriter();

            db = new DatabaseHelper();
            db.InitializeDatabase();

            lists = db.GetLists();

            /*
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
            }*/

            foreach (var list in lists)
            {
                listmode_combobox.Items.Add(list.Name);
            }

            if (lists.Count == 0)
            {
                listmode_combobox.Items.Add("无列表文件");
                listmode_combobox.IsEnabled = false;
                RandomButton.IsEnabled = false;
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
        /*
        public void InitializeLogWriter()
        {
            logWriter = new(userFolder + appFolder + logPath, true);
            logWriter.AutoFlush = true;
        }
        */
        private void FlashTimer_Tick(object sender, EventArgs e)
        {
            Random random = new Random();
            if (currectmode == "nummode")
            {
                Result.Text = random.Next(Convert.ToInt16(nummode_min.Value), Convert.ToInt16(nummode_max.Value)).ToString();

            }
            else if (currectmode == "listmode")
            {
                //Result.Text = item[listmode_combobox.SelectedIndex, random.Next(0, itemCount[listmode_combobox.SelectedIndex])];
                selectedItemIndex = random.Next(0,weightSum);
                Result.Text = currentListWithWeight[selectedItemIndex].Name;
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
            if (nummode_min.Value > nummode_max.Value)
            {
                (nummode_max.Value, nummode_min.Value) = (nummode_min.Value, nummode_max.Value);
            }
            if (currectmode == "listmode")
            {
                currentList = db.GetItemsByListId(lists[listmode_combobox.SelectedIndex].Id);
                UpdateListWithWeight();
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
            //logWriter.WriteLine(DateTime.Now.ToString() + " 被抽中的是:" + Result.Text);
            if (currectmode == "listmode")
            {
                db.IncreaseUsageCount(currentListWithWeight[selectedItemIndex].Id);
                db.WriteLog(DateTime.Now, currentListWithWeight[selectedItemIndex].ListId, currentListWithWeight[selectedItemIndex].Id);
                //logWriter.WriteLine(currentListWithWeight[selectedItemIndex].ListId.ToString()+";"+currentListWithWeight[selectedItemIndex].Id);
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
            modeChange();
            this.Visibility = Visibility.Hidden;
        }


        private void close_button_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            _autoToggleTimer.Stop();
            _flashTimer.Stop();
            //logWriter.Flush();
            //logWriter.Close();
            Application.Current.Shutdown();
        }

        public void UpdateListWithWeight()
        {
            weightSum = lists[listmode_combobox.SelectedIndex].WeightSum;
            currentListWithWeight = new Item[weightSum];
            int pos= 0;
            foreach (Item item in currentList)
            {
                for (int i = 0; i < item.Weight; i++)
                {
                    currentListWithWeight[pos] = item;
                    pos++;
                }
            }
        }
    }
}
