namespace BMATM.Data;

public class DatabaseContext : IDisposable
{
    private readonly string _connectionString;
    private SqliteConnection? _connection;

    public DatabaseContext()
    {
        var path = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath;
        var bmAtmPath = Path.GetDirectoryName(path);

        if (!Directory.Exists(bmAtmPath))
        {
            Directory.CreateDirectory(bmAtmPath);
        }

        var databasePath = Path.Combine(bmAtmPath, "bmatm.db");
        _connectionString = $"Data Source={databasePath}";
    }

    public async Task<SqliteConnection> GetConnectionAsync()
    {
        if (_connection == null)
        {
            _connection = new SqliteConnection(_connectionString);
            await _connection.OpenAsync();
            await InitializeDatabaseAsync();
        }

        return _connection;
    }

    private async Task InitializeDatabaseAsync()
    {
        if (_connection == null) return;

        // Create ATM table
        var createATMTable = @"
            CREATE TABLE IF NOT EXISTS ATMs (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                ATMNumber TEXT UNIQUE NOT NULL,
                ATMType INTEGER NOT NULL,
                GLAccount TEXT NOT NULL,
                BranchCode TEXT NOT NULL,
                BranchName TEXT NOT NULL,
                Location TEXT NOT NULL,
                Cassette1Denomination INTEGER NOT NULL,
                Cassette2Denomination INTEGER NOT NULL,
                Cassette3Denomination INTEGER NOT NULL,
                Cassette4Denomination INTEGER NOT NULL,
                Cassette1Balance INTEGER NOT NULL,
                Cassette2Balance INTEGER NOT NULL,
                Cassette3Balance INTEGER NOT NULL,
                Cassette4Balance INTEGER NOT NULL,
                IPAddress TEXT NOT NULL,
                IsActive BOOLEAN NOT NULL,
                InstallationDate TEXT NOT NULL,
                LastMaintenanceDate TEXT,
                Notes TEXT,
                CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                UpdatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
            )";

        // Create Users table
        var createUsersTable = @"
            CREATE TABLE IF NOT EXISTS Users (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Username TEXT UNIQUE NOT NULL,
                DisplayName TEXT NOT NULL,
                Email TEXT NOT NULL,
                Role INTEGER NOT NULL,
                Department TEXT NOT NULL,
                BranchNumber TEXT NOT NULL,
                BranchName TEXT NOT NULL,
                LastLogin TEXT,
                IsActive BOOLEAN NOT NULL DEFAULT 1,
                CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                UpdatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
            )";

        // Create SupervisorProfiles table
        var createSupervisorProfilesTable = @"
            CREATE TABLE IF NOT EXISTS SupervisorProfiles (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                UserId INTEGER NOT NULL,
                ProfileData TEXT NOT NULL,
                CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                UpdatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                FOREIGN KEY (UserId) REFERENCES Users (Id) ON DELETE CASCADE
            )";

        // Create triggers for UpdatedAt timestamps
        var createATMUpdateTrigger = @"
            CREATE TRIGGER IF NOT EXISTS UpdateATMTimestamp 
            AFTER UPDATE ON ATMs
            BEGIN
                UPDATE ATMs SET UpdatedAt = CURRENT_TIMESTAMP WHERE Id = NEW.Id;
            END";

        var createUsersUpdateTrigger = @"
            CREATE TRIGGER IF NOT EXISTS UpdateUsersTimestamp 
            AFTER UPDATE ON Users
            BEGIN
                UPDATE Users SET UpdatedAt = CURRENT_TIMESTAMP WHERE Id = NEW.Id;
            END";

        var createProfilesUpdateTrigger = @"
            CREATE TRIGGER IF NOT EXISTS UpdateProfilesTimestamp 
            AFTER UPDATE ON SupervisorProfiles
            BEGIN
                UPDATE SupervisorProfiles SET UpdatedAt = CURRENT_TIMESTAMP WHERE Id = NEW.Id;
            END";

        using var command = _connection.CreateCommand();

        command.CommandText = createATMTable;
        await command.ExecuteNonQueryAsync();

        command.CommandText = createUsersTable;
        await command.ExecuteNonQueryAsync();

        command.CommandText = createSupervisorProfilesTable;
        await command.ExecuteNonQueryAsync();
              
        command.CommandText = createATMUpdateTrigger;
        await command.ExecuteNonQueryAsync();

        command.CommandText = createUsersUpdateTrigger;
        await command.ExecuteNonQueryAsync();

        command.CommandText = createProfilesUpdateTrigger;
        await command.ExecuteNonQueryAsync();

        // Insert default data if tables are empty
        await SeedDefaultDataAsync();
    }

    private async Task SeedDefaultDataAsync()
    {
        if (_connection == null) return;

        // Check if Users table is empty
        using var checkUsersCommand = _connection.CreateCommand();
        checkUsersCommand.CommandText = "SELECT COUNT(*) FROM Users";
        var usersCount = Convert.ToInt32(await checkUsersCommand.ExecuteScalarAsync());

        if (usersCount == 0)
        {
            await SeedDefaultUsersAsync();
        }    

        // Check if ATMs table is empty
        using var checkATMCommand = _connection.CreateCommand();
        checkATMCommand.CommandText = "SELECT COUNT(*) FROM ATMs";
        var atmCount = Convert.ToInt32(await checkATMCommand.ExecuteScalarAsync());

        if (atmCount == 0)
        {
            await SeedDefaultATMsAsync();
        }
    }
    private async Task SeedDefaultUsersAsync()
    {
        if (_connection == null) return;

        var insertUser = @"
            INSERT INTO Users (Username, DisplayName, Email, Role, Department, BranchNumber, BranchName, IsActive)
            VALUES (@Username, @DisplayName, @Email, @Role, @Department, @BranchNumber, @BranchName, @IsActive)";

        using var command = _connection.CreateCommand();
        command.CommandText = insertUser;

        var defaultUsers = new[]
        {
            new { Username = "abdelrahmanhas", DisplayName = "Abdelrahman Hassan", Email = "abdelrahmanhas@banquemisr.com", Role = 1, Department = "West Cairo Region", BranchNumber = "707", BranchName = "Sheraton Cairo Branch" },
            new { Username = "admin", DisplayName = "Administrator", Email = "admin@bank.com", Role = 2, Department = "IT", BranchNumber = "001", BranchName = "Main Branch" },
            new { Username = "supervisor", DisplayName = "Branch Supervisor", Email = "supervisor@bank.com", Role = 1, Department = "Operations", BranchNumber = "002", BranchName = "Downtown Branch" },
            new { Username = "operator", DisplayName = "ATM Operator", Email = "operator@bank.com", Role = 0, Department = "Operations", BranchNumber = "003", BranchName = "Mall Branch" }
        };

        foreach (var user in defaultUsers)
        {
            command.Parameters.Clear();
            command.Parameters.AddWithValue("@Username", user.Username);
            command.Parameters.AddWithValue("@DisplayName", user.DisplayName);
            command.Parameters.AddWithValue("@Email", user.Email);
            command.Parameters.AddWithValue("@Role", user.Role);
            command.Parameters.AddWithValue("@Department", user.Department);
            command.Parameters.AddWithValue("@BranchNumber", user.BranchNumber);
            command.Parameters.AddWithValue("@BranchName", user.BranchName);
            command.Parameters.AddWithValue("@IsActive", true);
            await command.ExecuteNonQueryAsync();
        }
    }

    private async Task SeedDefaultATMsAsync()
    {
        if (_connection == null) return;

        var insertATM = @"
            INSERT INTO ATMs (ATMNumber, ATMType, GLAccount, BranchCode, BranchName, Location, 
                            Cassette1Denomination, Cassette2Denomination, Cassette3Denomination, Cassette4Denomination,
                            Cassette1Balance, Cassette2Balance, Cassette3Balance, Cassette4Balance,
                            IPAddress, IsActive, InstallationDate, LastMaintenanceDate, Notes)
            VALUES (@ATMNumber, @ATMType, @GLAccount, @BranchCode, @BranchName, @Location,
                   @Cassette1Denomination, @Cassette2Denomination, @Cassette3Denomination, @Cassette4Denomination,
                   @Cassette1Balance, @Cassette2Balance, @Cassette3Balance, @Cassette4Balance,
                   @IPAddress, @IsActive, @InstallationDate, @LastMaintenanceDate, @Notes)";

        using var command = _connection.CreateCommand();
        command.CommandText = insertATM;

        // Default ATM 1
        AddATMParameters(command, "ATM001", 0, "GL001", "BR001", "Main Branch", "Downtown Plaza",
                        20, 50, 100, 200, 50000, 100000, 200000, 400000,
                        "192.168.1.100", true, DateTime.Now.AddYears(-2), DateTime.Now.AddMonths(-3),
                        "Main branch ATM - high traffic location");
        await command.ExecuteNonQueryAsync();

        // Default ATM 2
        command.Parameters.Clear();
        AddATMParameters(command, "ATM002", 1, "GL002", "BR002", "Mall Branch", "Shopping Mall - Level 1",
                        20, 50, 100, 200, 30000, 75000, 150000, 300000,
                        "192.168.1.101", true, DateTime.Now.AddYears(-1), DateTime.Now.AddMonths(-2),
                        "Mall location - weekend peak usage");
        await command.ExecuteNonQueryAsync();
    }

    private void AddATMParameters(SqliteCommand command, string atmNumber, int atmType, string glAccount,
                                 string branchCode, string branchName, string location,
                                 int c1Denom, int c2Denom, int c3Denom, int c4Denom,
                                 int c1Balance, int c2Balance, int c3Balance, int c4Balance,
                                 string ipAddress, bool isActive, DateTime installationDate,
                                 DateTime lastMaintenanceDate, string notes)
    {
        command.Parameters.AddWithValue("@ATMNumber", atmNumber);
        command.Parameters.AddWithValue("@ATMType", atmType);
        command.Parameters.AddWithValue("@GLAccount", glAccount);
        command.Parameters.AddWithValue("@BranchCode", branchCode);
        command.Parameters.AddWithValue("@BranchName", branchName);
        command.Parameters.AddWithValue("@Location", location);
        command.Parameters.AddWithValue("@Cassette1Denomination", c1Denom);
        command.Parameters.AddWithValue("@Cassette2Denomination", c2Denom);
        command.Parameters.AddWithValue("@Cassette3Denomination", c3Denom);
        command.Parameters.AddWithValue("@Cassette4Denomination", c4Denom);
        command.Parameters.AddWithValue("@Cassette1Balance", c1Balance);
        command.Parameters.AddWithValue("@Cassette2Balance", c2Balance);
        command.Parameters.AddWithValue("@Cassette3Balance", c3Balance);
        command.Parameters.AddWithValue("@Cassette4Balance", c4Balance);
        command.Parameters.AddWithValue("@IPAddress", ipAddress);
        command.Parameters.AddWithValue("@IsActive", isActive);
        command.Parameters.AddWithValue("@InstallationDate", installationDate.ToString("yyyy-MM-dd HH:mm:ss"));
        command.Parameters.AddWithValue("@LastMaintenanceDate", lastMaintenanceDate.ToString("yyyy-MM-dd HH:mm:ss"));
        command.Parameters.AddWithValue("@Notes", notes);
    }
      
    public void Dispose()
    {
        _connection?.Close();
        _connection?.Dispose();
    }
}