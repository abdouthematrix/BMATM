namespace BMATM.Data;

// ==================== PROFILE REPOSITORY ====================
public class ProfileRepository : BaseRepository, IProfileRepository
{
    public static string RepositoryName = "Profiles";
    public ProfileRepository(DatabaseContext context) : base(context, RepositoryName) { }

    public override async Task InitializeAsync()
    {
        if (!await TableExistsAsync().ConfigureAwait(false))
        {
            string createTableQuery = @$"
                    CREATE TABLE {TableName} (          
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Username TEXT NOT NULL UNIQUE,
                        DisplayName TEXT,
                        Email TEXT,
                        Department TEXT,
                        BranchNumber TEXT,
                        BranchName TEXT,
                        Role INTEGER NOT NULL,
                        IsActive BOOLEAN NOT NULL DEFAULT 1,
                        CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        FOREIGN KEY (Username) REFERENCES {UserRepository.RepositoryName}(Username)
                    )";

            await _context.ExecuteNonQueryAsync(createTableQuery).ConfigureAwait(false);
        }
    }

    public async Task<SupervisorProfile> GetByUsernameAsync(string username)
    {
        _queryHelper.Reset();
        var query = _queryHelper.Select().Where("Username", username).Build();
        var parameters = _queryHelper.GetParameters();

        var profiles = await _context.ExecuteQueryAsync(query, parameters, MapProfile).ConfigureAwait(false);
        return profiles.FirstOrDefault();
    }

    public async Task<bool> CreateProfileAsync(SupervisorProfile profile)
    {
        _queryHelper.Reset();
        var values = new Dictionary<string, object>
            {
                { "Username", profile.User.Username },
                { "DisplayName", profile.User.DisplayName },
                { "Email", profile.User.Email },
                { "Department", profile.User.Department },
                { "BranchNumber", profile.User.BranchNumber },
                { "BranchName", profile.User.BranchName },
                { "Role", (int)profile.User.Role },
                { "IsActive", profile.User.IsActive }
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

    public async Task<bool> UpdateProfileAsync(SupervisorProfile profile)
    {
        _queryHelper.Reset();
        var values = new Dictionary<string, object>
            {
                { "DisplayName", profile.User.DisplayName },
                { "Email", profile.User.Email },
                { "Department", profile.User.Department },
                { "BranchNumber", profile.User.BranchNumber },
                { "BranchName", profile.User.BranchName },
                { "Role", (int)profile.User.Role },
                { "IsActive", profile.User.IsActive }
            };

        var query = _queryHelper.Update().Set(values).Where("Username", profile.User.Username).Build();
        var parameters = _queryHelper.GetParameters();

        var result = await _context.ExecuteNonQueryAsync(query, parameters).ConfigureAwait(false);
        return result > 0;
    }

    public async Task<List<SupervisorProfile>> GetAllProfilesAsync()
    {
        _queryHelper.Reset();
        var query = _queryHelper.Select().OrderBy("DisplayName").Build();
        var parameters = _queryHelper.GetParameters();

        return await _context.ExecuteQueryAsync(query, parameters, MapProfile).ConfigureAwait(false);
    }

    private SupervisorProfile MapProfile(SqliteDataReader reader)
    {
        return new SupervisorProfile
        {
            User = new User
            {
                Username = reader["Username"].ToString(),
                DisplayName = reader["DisplayName"].ToString(),
                Email = reader["Email"].ToString(),
                Department = reader["Department"].ToString(),
                BranchNumber = reader["BranchNumber"].ToString(),
                BranchName = reader["BranchName"].ToString(),
                Role = (UserRole)Convert.ToInt32(reader["Role"]),
                IsActive = Convert.ToBoolean(reader["IsActive"])
            }
        };
    }
}