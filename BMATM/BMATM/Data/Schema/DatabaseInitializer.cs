using System;
using System.Data.SQLite;
using BMATM.Core.Constants;
using BMATM.Data.Repositories;

namespace BMATM.Data.Schema
{
    public class DatabaseInitializer
    {
        private readonly SQLiteConnectionFactory _connectionFactory;

        public DatabaseInitializer(SQLiteConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        public void InitializeSchema()
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        CreateSupervisorsTable(connection);
                        CreateATMsTable(connection);
                        CreateATMTransactionsTable(connection);
                        CreateAuditLogTable(connection);
                        CreateIndexes(connection);

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public void SeedSampleData()
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                connection.Open();

                // Check if data already exists
                using (var checkCommand = new SQLiteCommand("SELECT COUNT(*) FROM Supervisors", connection))
                {
                    var count = Convert.ToInt32(checkCommand.ExecuteScalar());
                    if (count > 0)
                    {
                        return; // Data already exists, don't seed again
                    }
                }

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        SeedSupervisors(connection);
                        SeedATMs(connection);
                        SeedSampleTransactions(connection);

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        private void CreateSupervisorsTable(SQLiteConnection connection)
        {
            var sql = @"
                CREATE TABLE IF NOT EXISTS Supervisors (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Username TEXT NOT NULL UNIQUE,
                    PasswordHash TEXT NOT NULL,
                    FullName TEXT NOT NULL,
                    Email TEXT,
                    IsActive BOOLEAN DEFAULT 1,
                    CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
                    LastLoginDate DATETIME
                );";

            using (var command = new SQLiteCommand(sql, connection))
            {
                command.ExecuteNonQuery();
            }
        }

        private void CreateATMsTable(SQLiteConnection connection)
        {
            var sql = @"
                CREATE TABLE IF NOT EXISTS ATMs (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    SupervisorId INTEGER NOT NULL,
                    Name TEXT NOT NULL,
                    Branch TEXT NOT NULL,
                    GLNumber TEXT NOT NULL,
                    ATMType TEXT NOT NULL,
                    CassetteCount INTEGER NOT NULL,
                    IsActive BOOLEAN DEFAULT 1,
                    CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (SupervisorId) REFERENCES Supervisors(Id)
                );";

            using (var command = new SQLiteCommand(sql, connection))
            {
                command.ExecuteNonQuery();
            }
        }

        private void CreateATMTransactionsTable(SQLiteConnection connection)
        {
            var sql = @"
                CREATE TABLE IF NOT EXISTS ATMTransactions (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    ATMId INTEGER NOT NULL,
                    TransactionDate DATE NOT NULL,
                    BeginningCash DECIMAL(15,2),
                    AddedCash DECIMAL(15,2),
                    RecycledCash DECIMAL(15,2),
                    EndingCash DECIMAL(15,2),
                    DepositedCash DECIMAL(15,2),
                    GLBalance DECIMAL(15,2),
                    IsReconciled BOOLEAN DEFAULT 0,
                    ReconciliationStatus TEXT DEFAULT 'Pending',
                    Variance DECIMAL(15,2),
                    Notes TEXT,
                    CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (ATMId) REFERENCES ATMs(Id)
                );";

            using (var command = new SQLiteCommand(sql, connection))
            {
                command.ExecuteNonQuery();
            }
        }

        private void CreateAuditLogTable(SQLiteConnection connection)
        {
            var sql = @"
                CREATE TABLE IF NOT EXISTS AuditLog (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    TableName TEXT NOT NULL,
                    RecordId INTEGER NOT NULL,
                    Action TEXT NOT NULL,
                    OldValues TEXT,
                    NewValues TEXT,
                    UserId INTEGER,
                    Timestamp DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (UserId) REFERENCES Supervisors(Id)
                );";

            using (var command = new SQLiteCommand(sql, connection))
            {
                command.ExecuteNonQuery();
            }
        }

        private void CreateIndexes(SQLiteConnection connection)
        {
            var indexes = new[]
            {
                "CREATE INDEX IF NOT EXISTS IX_Supervisors_Username ON Supervisors(Username);",
                "CREATE INDEX IF NOT EXISTS IX_Supervisors_IsActive ON Supervisors(IsActive);",
                "CREATE INDEX IF NOT EXISTS IX_ATMs_SupervisorId ON ATMs(SupervisorId);",
                "CREATE INDEX IF NOT EXISTS IX_ATMs_Branch ON ATMs(Branch);",
                "CREATE INDEX IF NOT EXISTS IX_ATMs_GLNumber ON ATMs(GLNumber);",
                "CREATE INDEX IF NOT EXISTS IX_ATMs_IsActive ON ATMs(IsActive);",
                "CREATE INDEX IF NOT EXISTS IX_ATMTransactions_ATMId ON ATMTransactions(ATMId);",
                "CREATE INDEX IF NOT EXISTS IX_ATMTransactions_TransactionDate ON ATMTransactions(TransactionDate);",
                "CREATE INDEX IF NOT EXISTS IX_ATMTransactions_ReconciliationStatus ON ATMTransactions(ReconciliationStatus);",
                "CREATE INDEX IF NOT EXISTS IX_ATMTransactions_IsReconciled ON ATMTransactions(IsReconciled);",
                "CREATE INDEX IF NOT EXISTS IX_AuditLog_TableName_RecordId ON AuditLog(TableName, RecordId);",
                "CREATE INDEX IF NOT EXISTS IX_AuditLog_Timestamp ON AuditLog(Timestamp);"
            };

            foreach (var indexSql in indexes)
            {
                using (var command = new SQLiteCommand(indexSql, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        private void SeedSupervisors(SQLiteConnection connection)
        {
            var supervisors = new[]
            {
                new { Username = "abdelrahmanhas", Password = "password123", FullName = "Abdelrahman Hassan", Email = "abdelrahmanhas@banquemisr.com" },
                new { Username = "supervisor1", Password = "supervisor123", FullName = "Ahmed Mohamed", Email = "ahmed.mohamed@banquemisr.com" },
                new { Username = "supervisor2", Password = "supervisor123", FullName = "Fatima Ali", Email = "fatima.ali@banquemisr.com" }
            };

            foreach (var supervisor in supervisors)
            {
                var passwordHash = SupervisorRepository.HashPassword(supervisor.Password);
                var sql = @"
                    INSERT INTO Supervisors (Username, PasswordHash, FullName, Email, IsActive, CreatedDate)
                    VALUES (@username, @passwordHash, @fullName, @email, 1, @createdDate);";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@username", supervisor.Username);
                    command.Parameters.AddWithValue("@passwordHash", passwordHash);
                    command.Parameters.AddWithValue("@fullName", supervisor.FullName);
                    command.Parameters.AddWithValue("@email", supervisor.Email);
                    command.Parameters.AddWithValue("@createdDate", DateTime.Now);
                    command.ExecuteNonQuery();
                }
            }
        }

        private void SeedATMs(SQLiteConnection connection)
        {
            var atms = new[]
            {
                new { SupervisorId = 1, Name = "Sheraton Cairo Hotel ATM", Branch = "707", GLNumber = "101103576", ATMType = "DN", CassetteCount = 4 },
                new { SupervisorId = 1, Name = "Nile City Towers ATM", Branch = "707", GLNumber = "101103577", ATMType = "NCR", CassetteCount = 4 },
                new { SupervisorId = 1, Name = "Four Seasons Hotel ATM", Branch = "707", GLNumber = "101103578", ATMType = "DN", CassetteCount = 6 },
                new { SupervisorId = 2, Name = "Cairo International Airport ATM", Branch = "150", GLNumber = "101105001", ATMType = "Wincor", CassetteCount = 4 },
                new { SupervisorId = 2, Name = "Mall of Egypt ATM", Branch = "150", GLNumber = "101105002", ATMType = "Hyosung", CassetteCount = 4 },
                new { SupervisorId = 3, Name = "City Stars Mall ATM", Branch = "200", GLNumber = "101107001", ATMType = "DN", CassetteCount = 4 },
                new { SupervisorId = 3, Name = "Maadi Branch ATM", Branch = "200", GLNumber = "101107002", ATMType = "NCR", CassetteCount = 4 }
            };

            foreach (var atm in atms)
            {
                var sql = @"
                    INSERT INTO ATMs (SupervisorId, Name, Branch, GLNumber, ATMType, CassetteCount, IsActive, CreatedDate)
                    VALUES (@supervisorId, @name, @branch, @glNumber, @atmType, @cassetteCount, 1, @createdDate);";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@supervisorId", atm.SupervisorId);
                    command.Parameters.AddWithValue("@name", atm.Name);
                    command.Parameters.AddWithValue("@branch", atm.Branch);
                    command.Parameters.AddWithValue("@glNumber", atm.GLNumber);
                    command.Parameters.AddWithValue("@atmType", atm.ATMType);
                    command.Parameters.AddWithValue("@cassetteCount", atm.CassetteCount);
                    command.Parameters.AddWithValue("@createdDate", DateTime.Now);
                    command.ExecuteNonQuery();
                }
            }
        }

        private void SeedSampleTransactions(SQLiteConnection connection)
        {
            var random = new Random();
            var baseDate = DateTime.Today.AddDays(-30);

            // Create some sample transactions for testing
            for (int i = 0; i < 20; i++)
            {
                var transactionDate = baseDate.AddDays(random.Next(0, 30));
                var atmId = random.Next(1, 8); // ATM IDs 1-7
                var beginningCash = random.Next(50000, 200000);
                var addedCash = random.Next(0, 100000);
                var recycledCash = random.Next(0, 50000);
                var depositedCash = random.Next(10000, 80000);
                var endingCash = beginningCash + addedCash + recycledCash - depositedCash + random.Next(-1000, 1000);
                var variance = random.Next(-500, 500);

                string status;
                if (Math.Abs(variance) <= 1)
                    status = AppConstants.STATUS_BALANCED;
                else if (variance < 0)
                    status = AppConstants.STATUS_SHORTAGE;
                else
                    status = AppConstants.STATUS_OVER;

                var sql = @"
                    INSERT INTO ATMTransactions (
                        ATMId, TransactionDate, BeginningCash, AddedCash, RecycledCash, 
                        EndingCash, DepositedCash, IsReconciled, ReconciliationStatus, 
                        Variance, CreatedDate
                    ) VALUES (
                        @atmId, @transactionDate, @beginningCash, @addedCash, @recycledCash,
                        @endingCash, @depositedCash, @isReconciled, @reconciliationStatus,
                        @variance, @createdDate
                    );";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@atmId", atmId);
                    command.Parameters.AddWithValue("@transactionDate", transactionDate);
                    command.Parameters.AddWithValue("@beginningCash", beginningCash);
                    command.Parameters.AddWithValue("@addedCash", addedCash);
                    command.Parameters.AddWithValue("@recycledCash", recycledCash);
                    command.Parameters.AddWithValue("@endingCash", endingCash);
                    command.Parameters.AddWithValue("@depositedCash", depositedCash);
                    command.Parameters.AddWithValue("@isReconciled", status != AppConstants.STATUS_PENDING ? 1 : 0);
                    command.Parameters.AddWithValue("@reconciliationStatus", status);
                    command.Parameters.AddWithValue("@variance", variance);
                    command.Parameters.AddWithValue("@createdDate", DateTime.Now);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void DropAllTables()
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        var tables = new[] { "AuditLog", "ATMTransactions", "ATMs", "Supervisors" };

                        foreach (var table in tables)
                        {
                            using (var command = new SQLiteCommand($"DROP TABLE IF EXISTS {table}", connection))
                            {
                                command.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public void RecreateDatabase()
        {
            DropAllTables();
            InitializeSchema();
            SeedSampleData();
        }
    }
}