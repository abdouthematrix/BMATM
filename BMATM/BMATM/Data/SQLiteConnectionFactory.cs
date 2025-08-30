using System;
using System.Data.SQLite;
using System.IO;
using BMATM.Core.Constants;
using BMATM.Data.Schema;

namespace BMATM.Data
{
    public class SQLiteConnectionFactory
    {
        private readonly string _connectionString;
        private readonly string _databasePath;

        public SQLiteConnectionFactory() : this(GetDefaultDatabasePath()) { }

        public SQLiteConnectionFactory(string databasePath)
        {
            _databasePath = databasePath ?? throw new ArgumentNullException(nameof(databasePath));
            _connectionString = string.Format(AppConstants.CONNECTION_STRING_TEMPLATE, _databasePath);
        }

        public SQLiteConnection CreateConnection()
        {
            try
            {
                var connection = new SQLiteConnection(_connectionString);
                return connection;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(AppConstants.ERROR_DATABASE_CONNECTION, ex);
            }
        }

        public void InitializeDatabase()
        {
            try
            {
                // Create database file if it doesn't exist
                if (!DatabaseExists())
                {
                    CreateDatabaseFile();
                }

                // Initialize schema and sample data
                var initializer = new DatabaseInitializer(this);
                initializer.InitializeSchema();
                initializer.SeedSampleData();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to initialize database.", ex);
            }
        }

        public bool DatabaseExists()
        {
            return File.Exists(_databasePath);
        }

        public void DeleteDatabase()
        {
            if (DatabaseExists())
            {
                File.Delete(_databasePath);
            }
        }

        public string GetDatabasePath()
        {
            return _databasePath;
        }

        public long GetDatabaseSize()
        {
            if (!DatabaseExists())
                return 0;

            var fileInfo = new FileInfo(_databasePath);
            return fileInfo.Length;
        }

        public bool TestConnection()
        {
            try
            {
                using (var connection = CreateConnection())
                {
                    connection.Open();

                    // Execute a simple query to test connectivity
                    using (var command = new SQLiteCommand("SELECT 1", connection))
                    {
                        var result = command.ExecuteScalar();
                        return result != null && result.ToString() == "1";
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        private void CreateDatabaseFile()
        {
            // Ensure directory exists
            var directory = Path.GetDirectoryName(_databasePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Create empty database file
            SQLiteConnection.CreateFile(_databasePath);
        }

        private static string GetDefaultDatabasePath()
        {
            // Place database in application directory
            var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(appDirectory, AppConstants.DATABASE_FILENAME);
        }
    }
}