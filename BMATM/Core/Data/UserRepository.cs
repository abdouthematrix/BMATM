namespace BMATM.Data;

// ==================== USER REPOSITORY ====================
public class UserRepository : BaseRepository, IUserRepository
{
    public static string RepositoryName = "Users";
    public UserRepository(DatabaseContext context) : base(context, RepositoryName) { }
    public override async Task InitializeAsync()
    {
        if (!await TableExistsAsync().ConfigureAwait(false))
        {
            string createTableQuery = @$"
                    CREATE TABLE {TableName} (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Username TEXT NOT NULL UNIQUE,
                        PasswordHash TEXT NOT NULL,
                        Salt TEXT NOT NULL,
                        DisplayName TEXT,
                        Email TEXT,
                        Department TEXT,
                        BranchNumber TEXT,
                        BranchName TEXT,
                        Role INTEGER NOT NULL,
                        LastLogin DATETIME,
                        IsActive BOOLEAN NOT NULL DEFAULT 1,
                        CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
                    )";

            await _context.ExecuteNonQueryAsync(createTableQuery).ConfigureAwait(false);
            await SeedDefaultUsersAsync().ConfigureAwait(false);
        }
    }
    public async Task<User> GetByUsernameAsync(string username)
    {
        _queryHelper.Reset();
        var query = _queryHelper.Select().Where("Username", username).Build();
        var parameters = _queryHelper.GetParameters();

        var users = await _context.ExecuteQueryAsync(query, parameters, MapUser).ConfigureAwait(false);
        return users.FirstOrDefault();
    }
    public async Task<User> ValidateCredentialsAsync(string username, string password)
    {
        var user = await GetByUsernameAsync(username).ConfigureAwait(false);
        if (user == null || !user.IsActive) return null;

        _queryHelper.Reset();
        var query = _queryHelper.Select("Salt").Where("Username", username).Build();
        var parameters = _queryHelper.GetParameters();

        var salt = await _context.ExecuteScalarAsync<string>(query, parameters).ConfigureAwait(false);
        var hashedPassword = HashPassword(password, salt);

        _queryHelper.Reset();
        query = _queryHelper.Select("PasswordHash").Where("Username", username).Build();
        parameters = _queryHelper.GetParameters();

        var storedHash = await _context.ExecuteScalarAsync<string>(query, parameters).ConfigureAwait(false);

        return hashedPassword == storedHash ? user : null;
    }
    public async Task<bool> CreateUserAsync(User user, string password)
    {
        var salt = GenerateSalt();
        var hashedPassword = HashPassword(password, salt);

        _queryHelper.Reset();
        var values = new Dictionary<string, object>
            {
                { "Username", user.Username },
                { "PasswordHash", hashedPassword },
                { "Salt", salt },
                { "DisplayName", user.DisplayName },
                { "Email", user.Email },
                { "Department", user.Department },
                { "BranchNumber", user.BranchNumber },
                { "BranchName", user.BranchName },
                { "Role", (int)user.Role },
                { "IsActive", user.IsActive }
            };

        var query = _queryHelper.Insert().Values(values).Build();
        var parameters = _queryHelper.GetParameters();

        try
        {
            var result = await _context.ExecuteNonQueryAsync(query, parameters).ConfigureAwait(false);
            return result > 0;
        }
        catch
        {
            return false;
        }
    }
    public async Task<bool> UpdateUserAsync(User user)
    {
        _queryHelper.Reset();
        var values = new Dictionary<string, object>
            {
                { "DisplayName", user.DisplayName },
                { "Email", user.Email },
                { "Department", user.Department },
                { "BranchNumber", user.BranchNumber },
                { "BranchName", user.BranchName },
                { "Role", (int)user.Role },
                { "IsActive", user.IsActive }
            };

        var query = _queryHelper.Update().Set(values).Where("Username", user.Username).Build();
        var parameters = _queryHelper.GetParameters();

        var result = await _context.ExecuteNonQueryAsync(query, parameters).ConfigureAwait(false);
        return result > 0;
    }
    public async Task<bool> UpdateLastLoginAsync(string username)
    {
        _queryHelper.Reset();
        var values = new Dictionary<string, object> { { "LastLogin", DateTime.Now } };
        var query = _queryHelper.Update().Set(values).Where("Username", username).Build();
        var parameters = _queryHelper.GetParameters();

        var result = await _context.ExecuteNonQueryAsync(query, parameters).ConfigureAwait(false);
        return result > 0;
    }
    public async Task<List<User>> GetAllUsersAsync()
    {
        _queryHelper.Reset();
        var query = _queryHelper.Select().OrderBy("Username").Build();
        var parameters = _queryHelper.GetParameters();

        return await _context.ExecuteQueryAsync(query, parameters, MapUser).ConfigureAwait(false);
    }
    private async Task SeedDefaultUsersAsync()
    {
        var newUser = new User
        {
            Username = "abdelrahmanhas",
            DisplayName = "Abdelrahman Hassan",
            Email = "abdelrahmanhas@banquemisr.com",
            Department = "West Cairo Region",
            BranchNumber = "707",
            BranchName = "Sheraton Cairo",
            Role = UserRole.Supervisor,
            IsActive = true
        };

        await CreateUserAsync(newUser, "123").ConfigureAwait(false);

        var adminUser = new User
        {
            Username = "admin",
            DisplayName = "Administrator",
            Email = "admin@bank.com",
            Role = UserRole.Administrator,
            Department = "IT",
            BranchNumber = "001",
            BranchName = "Main Branch",
            IsActive = true
        };
        await CreateUserAsync(adminUser, "password").ConfigureAwait(false);
        var supervisorUser = new User
        {
            Username = "supervisor",
            DisplayName = "Branch Supervisor",
            Email = "supervisor@bank.com",
            Role = UserRole.Supervisor,
            Department = "Operations",
            BranchNumber = "002",
            BranchName = "Downtown Branch",
            IsActive = true
        };
        await CreateUserAsync(supervisorUser, "password").ConfigureAwait(false);
        var operatorUser = new User
        {
            Username = "operator",
            DisplayName = "ATM Operator",
            Email = "operator@bank.com",
            Role = UserRole.Operator,
            Department = "Operations",
            BranchNumber = "003",
            BranchName = "Mall Branch",
            IsActive = true
        };
        await CreateUserAsync(operatorUser, "password").ConfigureAwait(false);
    }
    private User MapUser(SqliteDataReader reader)
    {
        return new User
        {
            Username = reader["Username"].ToString(),
            DisplayName = reader["DisplayName"].ToString(),
            Email = reader["Email"].ToString(),
            Department = reader["Department"].ToString(),
            BranchNumber = reader["BranchNumber"].ToString(),
            BranchName = reader["BranchName"].ToString(),
            Role = (UserRole)Convert.ToInt32(reader["Role"]),
            LastLogin = reader["LastLogin"] != DBNull.Value ? Convert.ToDateTime(reader["LastLogin"]) : DateTime.MinValue,
            IsActive = Convert.ToBoolean(reader["IsActive"])
        };
    }
    private string GenerateSalt()
    {
        using var rng = RandomNumberGenerator.Create();
        var saltBytes = new byte[32];
        rng.GetBytes(saltBytes);
        return Convert.ToBase64String(saltBytes);
    }
    private string HashPassword(string password, string salt)
    {
        using var pbkdf2 = new Rfc2898DeriveBytes(password, Convert.FromBase64String(salt), 10000);
        return Convert.ToBase64String(pbkdf2.GetBytes(32));
    }
}