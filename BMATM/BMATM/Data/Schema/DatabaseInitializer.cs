using System;
using System.Data.SQLite;
using System.Security.Cryptography;
using System.Text;

namespace BMATM.Data.Schema
{
    public class DatabaseInitializer
    {
        private readonly SQLiteConnectionFactory _connectionFactory;

        public DatabaseInitializer(SQLiteConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        public void InitializeDatabase()
        {
            CreateTables();
            SeedInitialData();
        }

        private void CreateTables()
        {
            string[] createTableScripts = {
                CreateSupervisorsTable(),
                CreateATMsTable(),
                CreateATMTransactionsTable(),
                CreateAuditLogTable()
            };

            using (var connection = _connectionFactory.CreateConnection())
            {
                connection.Open();

                foreach (var script in createTableScripts)
                {
                    using (var command = new SQLiteCommand(script, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        private void SeedInitialData()
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                connection.Open();

                // Check if data already exists
                using (var checkCommand = new SQLiteCommand("SELECT COUNT(*) FROM Supervisors", connection))
                {
                    var count = Convert.ToInt32(checkCommand.ExecuteScalar());
                    if (count > 0)
                        return; // Data already exists
                }

                // Insert sample supervisor
                string passwordHash = HashPassword("password123");
                string insertSupervisor = @"
                    INSERT INTO Supervisors (Username, PasswordHash, FullName, Email, IsActive, CreatedDate) 
                    VALUES (@Username, @PasswordHash, @FullName, @Email, @IsActive, @CreatedDate)";

                using (var command = new SQLiteCommand(insertSupervisor, connection))
                {
                    command.Parameters.AddWithValue("@Username", "abdelrahmanhas");
                    command.Parameters.AddWithValue("@PasswordHash", passwordHash);
                    command.Parameters.AddWithValue("@FullName", "Abdelrahman Hassan");
                    command.Parameters.AddWithValue("@Email", "abdelrahmanhas@banquemisr.com");
                    command.Parameters.AddWithValue("@IsActive", true);
                    command.Parameters.AddWithValue("@CreatedDate", DateTime.Now);
                    command.ExecuteNonQuery();
                }

                // Get the supervisor ID
                long supervisorId;
                using (var command = new SQLiteCommand("SELECT last_insert_rowid()", connection))
                {
                    supervisorId = (long)command.ExecuteScalar();
                }

                // Insert sample ATMs
                string insertATM = @"
                    INSERT INTO ATMs (SupervisorId, Name, Branch, GLNumber, ATMType, CassetteCount, IsActive, CreatedDate) 
                    VALUES (@SupervisorId, @Name, @Branch, @GLNumber, @ATMType, @CassetteCount, @IsActive, @CreatedDate)";

                var sampleATMs = new[]
                {
                    new { Name = "Sheraton Cairo Hotel ATM", Branch = "707", GLNumber = "101103576", ATMType = "DN", CassetteCount = 4 },
                    new { Name = "New Cairo Branch ATM 1", Branch = "708", GLNumber = "101103577", ATMType = "TT", CassetteCount = 6 },
                    new { Name = "Maadi Branch ATM", Branch = "709", GLNumber = "101103578", ATMType = "DN", CassetteCount = 4 }
                };

                foreach (var atm in sampleATMs)
                {
                    using (var command = new SQLiteCommand(insertATM, connection))
                    {
                        command.Parameters.AddWithValue("@SupervisorId", supervisorId);
                        command.Parameters.AddWithValue("@Name", atm.Name);
                        command.Parameters.AddWithValue("@Branch", atm.Branch);
                        command.Parameters.AddWithValue("@GLNumber", atm.GLNumber);
                        command.Parameters.AddWithValue("@ATMType", atm.ATMType);
                        command.Parameters.AddWithValue("@CassetteCount", atm.CassetteCount);
                        command.Parameters.AddWithValue("@IsActive", true);
                        command.Parameters.AddWithValue("@CreatedDate", DateTime.Now);
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        private string CreateSupervisorsTable()
        {
            return @"
                CREATE TABLE IF NOT EXISTS Supervisors (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Username TEXT NOT NULL UNIQUE,
                    PasswordHash TEXT NOT NULL,
                    FullName TEXT NOT NULL,
                    Email TEXT,
                    IsActive BOOLEAN DEFAULT 1,
                    CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
                    LastLoginDate DATETIME
                )";
        }

        private string CreateATMsTable()
        {
            return @"
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
                )";
        }

        private string CreateATMTransactionsTable()
        {
            return @"
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
                    ReconciliationStatus TEXT,
                    Variance DECIMAL(15,2),
                    Notes TEXT,
                    CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (ATMId) REFERENCES ATMs(Id)
                )";
        }

        private string CreateAuditLogTable()
        {
            return @"
                CREATE TABLE IF NOT EXISTS AuditLog (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    TableName TEXT NOT NULL,
                    RecordId INTEGER NOT NULL,
                    Action TEXT NOT NULL,
                    OldValues TEXT,
                    NewValues TEXT,
                    UserId INTEGER,
                    Timestamp DATETIME DEFAULT CURRENT_TIMESTAMP
                )";
        }

        private string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                return string.Empty;

            using (var sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "BMATM_SALT"));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}