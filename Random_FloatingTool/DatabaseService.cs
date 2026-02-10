using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Data.Sqlite;

namespace Random_FloatingTool
{
    /// <summary>
    /// SQLite 数据库服务，负责列表和日志的持久化存储
    /// </summary>
    public class DatabaseService : IDisposable
    {
        private readonly SqliteConnection _connection;

        /// <summary>
        /// 初始化数据库服务，自动创建数据库文件和表结构
        /// </summary>
        /// <param name="dbPath">数据库文件的完整路径</param>
        public DatabaseService(string dbPath)
        {
            // 确保目录存在
            string dir = Path.GetDirectoryName(dbPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            _connection = new SqliteConnection($"Data Source={dbPath}");
            _connection.Open();

            // 启用外键约束
            using var pragmaCmd = _connection.CreateCommand();
            pragmaCmd.CommandText = "PRAGMA foreign_keys = ON;";
            pragmaCmd.ExecuteNonQuery();

            InitializeTables();
        }

        /// <summary>
        /// 创建数据库表结构
        /// </summary>
        private void InitializeTables()
        {
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS lists (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    name TEXT NOT NULL,
                    sort_order INTEGER DEFAULT 0,
                    selected_count INTEGER DEFAULT 0
                );

                CREATE TABLE IF NOT EXISTS list_items (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    list_id INTEGER NOT NULL,
                    content TEXT NOT NULL,
                    sort_order INTEGER DEFAULT 0,
                    selected_count INTEGER DEFAULT 0,
                    FOREIGN KEY (list_id) REFERENCES lists(id) ON DELETE CASCADE
                );

                CREATE TABLE IF NOT EXISTS logs (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    timestamp TEXT NOT NULL,
                    mode TEXT NOT NULL,
                    list_id INTEGER,
                    item_id INTEGER,
                    result_number INTEGER,
                    FOREIGN KEY (list_id) REFERENCES lists(id),
                    FOREIGN KEY (item_id) REFERENCES list_items(id)
                );
            ";
            cmd.ExecuteNonQuery();
        }

        #region 列表操作

        /// <summary>
        /// 获取所有列表组（按 sort_order 排序）
        /// </summary>
        /// <returns>列表组列表，每项包含 (Id, Name)</returns>
        public List<(int Id, string Name)> GetAllGroups()
        {
            var groups = new List<(int, string)>();
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = "SELECT id, name FROM lists ORDER BY sort_order, id;";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                groups.Add((reader.GetInt32(0), reader.GetString(1)));
            }
            return groups;
        }

        /// <summary>
        /// 获取指定列表组的所有项（按 sort_order 排序）
        /// </summary>
        /// <param name="listId">列表组 ID</param>
        /// <returns>列表项列表，每项包含 (Id, Content)</returns>
        public List<(int Id, string Content)> GetItemsByGroup(int listId)
        {
            var items = new List<(int, string)>();
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = "SELECT id, content FROM list_items WHERE list_id = @listId ORDER BY sort_order, id;";
            cmd.Parameters.AddWithValue("@listId", listId);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                items.Add((reader.GetInt32(0), reader.GetString(1)));
            }
            return items;
        }

        /// <summary>
        /// 添加列表组
        /// </summary>
        /// <param name="name">组名</param>
        /// <returns>新组的 ID</returns>
        public int AddGroup(string name)
        {
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO lists (name, sort_order) 
                VALUES (@name, (SELECT COALESCE(MAX(sort_order), 0) + 1 FROM lists));
                SELECT last_insert_rowid();";
            cmd.Parameters.AddWithValue("@name", name);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        /// <summary>
        /// 添加列表项
        /// </summary>
        /// <param name="listId">所属组 ID</param>
        /// <param name="content">项内容</param>
        /// <returns>新项的 ID</returns>
        public int AddItem(int listId, string content)
        {
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO list_items (list_id, content, sort_order)
                VALUES (@listId, @content, (SELECT COALESCE(MAX(sort_order), 0) + 1 FROM list_items WHERE list_id = @listId));
                SELECT last_insert_rowid();";
            cmd.Parameters.AddWithValue("@listId", listId);
            cmd.Parameters.AddWithValue("@content", content);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        #endregion

        #region 日志操作

        /// <summary>
        /// 写入列表模式的抽取日志，并增加选中计数
        /// </summary>
        /// <param name="listId">列表组 ID</param>
        /// <param name="itemId">被抽中的项 ID</param>
        public void AddListModeLog(int listId, int itemId)
        {
            using var transaction = _connection.BeginTransaction();
            try 
            {
                using (var cmd = _connection.CreateCommand())
                {
                    cmd.Transaction = transaction;
                    cmd.CommandText = @"
                        INSERT INTO logs (timestamp, mode, list_id, item_id)
                        VALUES (@timestamp, 'listmode', @listId, @itemId);";
                    cmd.Parameters.AddWithValue("@timestamp", DateTime.Now.ToString("o"));
                    cmd.Parameters.AddWithValue("@listId", listId);
                    cmd.Parameters.AddWithValue("@itemId", itemId);
                    cmd.ExecuteNonQuery();
                }

                using (var cmd = _connection.CreateCommand())
                {
                    cmd.Transaction = transaction;
                    cmd.CommandText = "UPDATE lists SET selected_count = selected_count + 1 WHERE id = @listId;";
                    cmd.Parameters.AddWithValue("@listId", listId);
                    cmd.ExecuteNonQuery();
                }

                using (var cmd = _connection.CreateCommand())
                {
                    cmd.Transaction = transaction;
                    cmd.CommandText = "UPDATE list_items SET selected_count = selected_count + 1 WHERE id = @itemId;";
                    cmd.Parameters.AddWithValue("@itemId", itemId);
                    cmd.ExecuteNonQuery();
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        /// <summary>
        /// 写入数字模式的抽取日志
        /// </summary>
        /// <param name="resultNumber">被抽中的数字</param>
        public void AddNumModeLog(int resultNumber)
        {
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO logs (timestamp, mode, result_number)
                VALUES (@timestamp, 'nummode', @resultNumber);";
            cmd.Parameters.AddWithValue("@timestamp", DateTime.Now.ToString("o"));
            cmd.Parameters.AddWithValue("@resultNumber", resultNumber);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// 获取所有日志记录
        /// </summary>
        public List<(int Id, string Timestamp, string Mode, int? GroupId, int? ItemId, int? ResultNumber)> GetLogs()
        {
            var logs = new List<(int, string, string, int?, int?, int?)>();
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = "SELECT id, timestamp, mode, list_id, item_id, result_number FROM logs ORDER BY id DESC;";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                logs.Add((
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.GetString(2),
                    reader.IsDBNull(3) ? null : reader.GetInt32(3),
                    reader.IsDBNull(4) ? null : reader.GetInt32(4),
                    reader.IsDBNull(5) ? null : reader.GetInt32(5)
                ));
            }
            return logs;
        }

        #endregion

        public void Dispose()
        {
            _connection?.Close();
            _connection?.Dispose();
        }
    }
}
