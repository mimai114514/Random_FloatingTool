using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Data.Sqlite;

namespace Random_FloatingTool
{
    public class DatabaseManager
    {
        private string _dbPath;
        private string _connectionString;

        public DatabaseManager(string dbFolder)
        {
            _dbPath = Path.Combine(dbFolder, "data.db");
            _connectionString = $"Data Source={_dbPath}";
        }

        public void InitializeDatabase()
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Lists (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL
                    );

                    CREATE TABLE IF NOT EXISTS Items (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        ListId INTEGER NOT NULL,
                        Content TEXT NOT NULL,
                        FOREIGN KEY(ListId) REFERENCES Lists(Id)
                    );

                    CREATE TABLE IF NOT EXISTS Logs (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Timestamp TEXT NOT NULL,
                        Content TEXT NOT NULL
                    );
                ";
                command.ExecuteNonQuery();
            }
        }

        public void AddLog(string timestamp, string content)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "INSERT INTO Logs (Timestamp, Content) VALUES ($timestamp, $content)";
                command.Parameters.AddWithValue("$timestamp", timestamp);
                command.Parameters.AddWithValue("$content", content);
                command.ExecuteNonQuery();
            }
        }

        public void ImportLogsFromText(string logFilePath)
        {
            if (!File.Exists(logFilePath)) return;

            // Check if logs table is empty to avoid duplicate import
            bool isLogEmpty = false;
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT COUNT(*) FROM Logs";
                var count = (long)command.ExecuteScalar();
                if (count == 0) isLogEmpty = true;
            }

            if (isLogEmpty)
            {
                var lines = File.ReadAllLines(logFilePath);
                using (var connection = new SqliteConnection(_connectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        var command = connection.CreateCommand();
                        command.Transaction = transaction;
                        command.CommandText = "INSERT INTO Logs (Timestamp, Content) VALUES ($timestamp, $content)";

                        var pTimestamp = command.CreateParameter();
                        pTimestamp.ParameterName = "$timestamp";
                        command.Parameters.Add(pTimestamp);

                        var pContent = command.CreateParameter();
                        pContent.ParameterName = "$content";
                        command.Parameters.Add(pContent);

                        foreach (var line in lines)
                        {
                            // Try to parse timestamp.
                            // Format in ToolBox.xaml.cs: DateTime.Now.ToString() + " " + Result_Side.Text + Result.Text
                            // Example: "2023/10/27 10:00:00 被抽中的是:XX"
                            // We will try to find the first space after a reasonable date length, or just split by first space.
                            // DateTime.Now.ToString() usually contains a space between date and time.
                            // Let's assume the timestamp is everything before the SECOND space?
                            // Or simpler: just put the whole line in content?
                            // User requirement: "Store logs (timestamp and content)".
                            // Let's try to split by the first occurrence of " 被抽中的是" or similar, or just heuristic.

                            string timestamp = DateTime.Now.ToString(); // Default
                            string content = line;

                            // Simple heuristic: Try to parse the first part as date
                            // But DateTime.Now.ToString() format varies heavily.
                            // If we can't reliably parse, we might store the whole line in Content and Current Time in Timestamp?
                            // Or better: store the whole line as Content, and Timestamp as empty or parsed if possible.

                            // Let's try to look for the known separator " 被抽中的是"
                            // Result_Side.Text is usually "被抽中的是..." or "被抽中的是:"

                            int separatorIndex = line.IndexOf(" 被抽中的是");
                            if (separatorIndex > 0)
                            {
                                timestamp = line.Substring(0, separatorIndex);
                                content = line.Substring(separatorIndex + 1); // Skip the space
                            }
                            else
                            {
                                // Fallback
                                timestamp = DateTime.Now.ToString();
                            }

                            pTimestamp.Value = timestamp;
                            pContent.Value = content;
                            command.ExecuteNonQuery();
                        }
                        transaction.Commit();
                    }
                }
            }
        }

        public List<string> GetListNames()
        {
            var listNames = new List<string>();
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT Name FROM Lists ORDER BY Id";
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        listNames.Add(reader.GetString(0));
                    }
                }
            }
            return listNames;
        }

        public List<List<string>> GetAllListItems()
        {
            var allItems = new List<List<string>>();
            // We need to get items grouped by list, in the order of lists.
            // First get all lists.
            var listIds = new List<long>();
             using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT Id FROM Lists ORDER BY Id";
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        listIds.Add(reader.GetInt64(0));
                    }
                }
            }

            foreach(var listId in listIds)
            {
                var items = new List<string>();
                using (var connection = new SqliteConnection(_connectionString))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = "SELECT Content FROM Items WHERE ListId = $listId ORDER BY Id";
                    command.Parameters.AddWithValue("$listId", listId);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            items.Add(reader.GetString(0));
                        }
                    }
                }
                allItems.Add(items);
            }

            return allItems;
        }
    }
}
