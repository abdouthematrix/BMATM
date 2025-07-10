namespace BMATM.Data.Repositories;

public interface ISupervisorProfileRepository : IBaseRepository<SupervisorProfile>
{
    Task<SupervisorProfile?> GetByUsernameAsync(string username);
    Task<bool> ExistsByUsernameAsync(string username);
    Task<bool> DeleteByUsernameAsync(string username);
    Task<IEnumerable<SupervisorProfile>> GetAllProfilesWithUserDataAsync();
}

public class SupervisorProfileRepository : BaseRepository<SupervisorProfile>, ISupervisorProfileRepository
{
    public SupervisorProfileRepository(DatabaseContext context) : base(context, "SupervisorProfiles")
    {
    }

    public async Task<SupervisorProfile?> GetByUsernameAsync(string username)
    {
        var connection = await _context.GetConnectionAsync();
        using var command = connection.CreateCommand();
        command.CommandText = @$"
            SELECT sp.*, u.* 
            FROM {_tableName} sp
            INNER JOIN Users u ON sp.UserId = u.Id
            WHERE u.Username = @Username COLLATE NOCASE";
        command.Parameters.AddWithValue("@Username", username);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return await MapReaderToEntityWithUserAsync(reader);
        }

        return null;
    }

    public async Task<bool> ExistsByUsernameAsync(string username)
    {
        var connection = await _context.GetConnectionAsync();
        using var command = connection.CreateCommand();
        command.CommandText = @$"
            SELECT COUNT(*) 
            FROM {_tableName} sp
            INNER JOIN Users u ON sp.UserId = u.Id
            WHERE u.Username = @Username COLLATE NOCASE";
        command.Parameters.AddWithValue("@Username", username);

        var count = Convert.ToInt32(await command.ExecuteScalarAsync());
        return count > 0;
    }

    public async Task<bool> DeleteByUsernameAsync(string username)
    {
        var connection = await _context.GetConnectionAsync();
        using var command = connection.CreateCommand();
        command.CommandText = @$"
            DELETE FROM {_tableName} 
            WHERE UserId IN (
                SELECT Id FROM Users WHERE Username = @Username COLLATE NOCASE
            )";
        command.Parameters.AddWithValue("@Username", username);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<IEnumerable<SupervisorProfile>> GetAllProfilesWithUserDataAsync()
    {
        var connection = await _context.GetConnectionAsync();
        using var command = connection.CreateCommand();
        command.CommandText = @$"
            SELECT sp.*, u.* 
            FROM {_tableName} sp
            INNER JOIN Users u ON sp.UserId = u.Id
            ORDER BY u.DisplayName";

        using var reader = await command.ExecuteReaderAsync();
        var results = new List<SupervisorProfile>();

        while (await reader.ReadAsync())
        {
            var entity = await MapReaderToEntityWithUserAsync(reader);
            if (entity != null)
                results.Add(entity);
        }

        return results;
    }

    public override async Task<SupervisorProfile?> AddAsync(SupervisorProfile entity)
    {
        // First, ensure the user exists and get their ID
        var userId = await GetUserIdByUsernameAsync(entity.User?.Username);
        if (userId == null)
        {
            throw new InvalidOperationException($"User with username '{entity.User?.Username}' not found");
        }

        var connection = await _context.GetConnectionAsync();
        using var command = connection.CreateCommand();
        command.CommandText = @$"
            INSERT INTO {_tableName} (UserId, ProfileData)
            VALUES (@UserId, @ProfileData);
            SELECT last_insert_rowid();";

        command.Parameters.AddWithValue("@UserId", userId.Value);
        command.Parameters.AddWithValue("@ProfileData", SerializeToJson(entity));

        var id = await command.ExecuteScalarAsync();
        if (id != null && long.TryParse(id.ToString(), out var newId))
        {
            return await GetByIdAsync((int)newId);
        }

        return null;
    }

    public override async Task<SupervisorProfile?> UpdateAsync(SupervisorProfile entity)
    {
        var userId = await GetUserIdByUsernameAsync(entity.User?.Username);
        if (userId == null)
        {
            throw new InvalidOperationException($"User with username '{entity.User?.Username}' not found");
        }

        var connection = await _context.GetConnectionAsync();
        using var command = connection.CreateCommand();
        command.CommandText = @$"
            UPDATE {_tableName} 
            SET ProfileData = @ProfileData
            WHERE UserId = @UserId";

        command.Parameters.AddWithValue("@UserId", userId.Value);
        command.Parameters.AddWithValue("@ProfileData", SerializeToJson(entity));

        var rowsAffected = await command.ExecuteNonQueryAsync();
        if (rowsAffected > 0)
        {
            return await GetByUsernameAsync(entity.User?.Username ?? string.Empty);
        }

        return null;
    }

    private async Task<int?> GetUserIdByUsernameAsync(string? username)
    {
        if (string.IsNullOrEmpty(username))
            return null;

        var connection = await _context.GetConnectionAsync();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id FROM Users WHERE Username = @Username COLLATE NOCASE";
        command.Parameters.AddWithValue("@Username", username);

        var result = await command.ExecuteScalarAsync();
        return result != null ? Convert.ToInt32(result) : null;
    }

    private async Task<SupervisorProfile?> MapReaderToEntityWithUserAsync(SqliteDataReader reader)
    {
        var profileData = GetString(reader, "ProfileData");
        var profile = DeserializeFromJson<SupervisorProfile>(profileData);

        if (profile == null)
        {
            profile = new SupervisorProfile();
        }

        // Map user data from the joined table
        profile.User = new User
        {
            Username = GetString(reader, "Username"),
            DisplayName = GetString(reader, "DisplayName"),
            Email = GetString(reader, "Email"),
            Role = (UserRole)GetInt32(reader, "Role"),
            Department = GetString(reader, "Department"),
            BranchNumber = GetString(reader, "BranchNumber"),
            BranchName = GetString(reader, "BranchName"),
            LastLogin = GetDateTimeOrDefault(reader, "LastLogin"),
            IsActive = GetBoolean(reader, "IsActive")
        };

        return profile;
    }

    protected override async Task<SupervisorProfile?> MapReaderToEntityAsync(SqliteDataReader reader)
    {
        var profileData = GetString(reader, "ProfileData");
        var profile = DeserializeFromJson<SupervisorProfile>(profileData);

        if (profile == null)
        {
            profile = new SupervisorProfile();
        }

        // Note: This won't have user data populated since it's not joined
        // Use GetByUsernameAsync or GetAllProfilesWithUserDataAsync for complete data
        return profile;
    }

    protected override (string sql, Dictionary<string, object?> parameters) GetInsertSqlAndParameters(SupervisorProfile entity)
    {
        var sql = @$"
            INSERT INTO {_tableName} (UserId, ProfileData)
            VALUES (@UserId, @ProfileData);
            SELECT last_insert_rowid();";

        var parameters = new Dictionary<string, object?>
        {
            ["@UserId"] = 0, // This will be set dynamically in AddAsync
            ["@ProfileData"] = SerializeToJson(entity)
        };

        return (sql, parameters);
    }

    protected override (string sql, Dictionary<string, object?> parameters) GetUpdateSqlAndParameters(SupervisorProfile entity)
    {
        var sql = @$"
            UPDATE {_tableName} 
            SET ProfileData = @ProfileData
            WHERE Id = @Id";

        var parameters = new Dictionary<string, object?>
        {
            ["@Id"] = GetEntityId(entity),
            ["@ProfileData"] = SerializeToJson(entity)
        };

        return (sql, parameters);
    }

    protected override int GetEntityId(SupervisorProfile entity)
    {
        // SupervisorProfile model also needs an Id property
        throw new NotImplementedException("SupervisorProfile model needs an Id property for this operation");
    }
}