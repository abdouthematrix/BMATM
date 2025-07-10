namespace BMATM.Data.Repositories;

public interface IUserRepository : IBaseRepository<User>
{
    Task<User?> GetByUsernameAsync(string username);
    Task<bool> ExistsByUsernameAsync(string username);
    Task<IEnumerable<User>> GetByRoleAsync(UserRole role);
    Task<IEnumerable<User>> GetByDepartmentAsync(string department);
    Task<IEnumerable<User>> GetActiveSupervisorsAsync();
    Task<bool> UpdateLastLoginAsync(string username, DateTime lastLogin);
    Task<bool> DeactivateUserAsync(string username);
    Task<bool> ActivateUserAsync(string username);
}

public class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(DatabaseContext context) : base(context, "Users")
    {
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        var connection = await _context.GetConnectionAsync();
        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT * FROM {_tableName} WHERE Username = @Username COLLATE NOCASE";
        command.Parameters.AddWithValue("@Username", username);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return await MapReaderToEntityAsync(reader);
        }

        return null;
    }

    public async Task<bool> ExistsByUsernameAsync(string username)
    {
        var connection = await _context.GetConnectionAsync();
        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT COUNT(*) FROM {_tableName} WHERE Username = @Username COLLATE NOCASE";
        command.Parameters.AddWithValue("@Username", username);

        var count = Convert.ToInt32(await command.ExecuteScalarAsync());
        return count > 0;
    }

    public async Task<IEnumerable<User>> GetByRoleAsync(UserRole role)
    {
        var connection = await _context.GetConnectionAsync();
        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT * FROM {_tableName} WHERE Role = @Role AND IsActive = 1 ORDER BY DisplayName";
        command.Parameters.AddWithValue("@Role", (int)role);

        using var reader = await command.ExecuteReaderAsync();
        var results = new List<User>();

        while (await reader.ReadAsync())
        {
            var entity = await MapReaderToEntityAsync(reader);
            if (entity != null)
                results.Add(entity);
        }

        return results;
    }

    public async Task<IEnumerable<User>> GetByDepartmentAsync(string department)
    {
        var connection = await _context.GetConnectionAsync();
        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT * FROM {_tableName} WHERE Department = @Department AND IsActive = 1 ORDER BY DisplayName";
        command.Parameters.AddWithValue("@Department", department);

        using var reader = await command.ExecuteReaderAsync();
        var results = new List<User>();

        while (await reader.ReadAsync())
        {
            var entity = await MapReaderToEntityAsync(reader);
            if (entity != null)
                results.Add(entity);
        }

        return results;
    }

    public async Task<IEnumerable<User>> GetActiveSupervisorsAsync()
    {
        var connection = await _context.GetConnectionAsync();
        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT * FROM {_tableName} WHERE Role = @Role AND IsActive = 1 ORDER BY DisplayName";
        command.Parameters.AddWithValue("@Role", (int)UserRole.Supervisor);

        using var reader = await command.ExecuteReaderAsync();
        var results = new List<User>();

        while (await reader.ReadAsync())
        {
            var entity = await MapReaderToEntityAsync(reader);
            if (entity != null)
                results.Add(entity);
        }

        return results;
    }

    public async Task<bool> UpdateLastLoginAsync(string username, DateTime lastLogin)
    {
        var connection = await _context.GetConnectionAsync();
        using var command = connection.CreateCommand();
        command.CommandText = @$"
            UPDATE {_tableName} 
            SET LastLogin = @LastLogin
            WHERE Username = @Username COLLATE NOCASE";

        command.Parameters.AddWithValue("@Username", username);
        command.Parameters.AddWithValue("@LastLogin", lastLogin.ToString("yyyy-MM-dd HH:mm:ss"));

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> DeactivateUserAsync(string username)
    {
        var connection = await _context.GetConnectionAsync();
        using var command = connection.CreateCommand();
        command.CommandText = @$"
            UPDATE {_tableName} 
            SET IsActive = 0
            WHERE Username = @Username COLLATE NOCASE";

        command.Parameters.AddWithValue("@Username", username);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> ActivateUserAsync(string username)
    {
        var connection = await _context.GetConnectionAsync();
        using var command = connection.CreateCommand();
        command.CommandText = @$"
            UPDATE {_tableName} 
            SET IsActive = 1
            WHERE Username = @Username COLLATE NOCASE";

        command.Parameters.AddWithValue("@Username", username);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    protected override async Task<User?> MapReaderToEntityAsync(SqliteDataReader reader)
    {
        return new User
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
    }

    protected override (string sql, Dictionary<string, object?> parameters) GetInsertSqlAndParameters(User entity)
    {
        var sql = @$"
            INSERT INTO {_tableName} (Username, DisplayName, Email, Role, Department, BranchNumber, BranchName, LastLogin, IsActive)
            VALUES (@Username, @DisplayName, @Email, @Role, @Department, @BranchNumber, @BranchName, @LastLogin, @IsActive);
            SELECT last_insert_rowid();";

        var parameters = new Dictionary<string, object?>
        {
            ["@Username"] = entity.Username,
            ["@DisplayName"] = entity.DisplayName,
            ["@Email"] = entity.Email,
            ["@Role"] = (int)entity.Role,
            ["@Department"] = entity.Department,
            ["@BranchNumber"] = entity.BranchNumber,
            ["@BranchName"] = entity.BranchName,
            ["@LastLogin"] = entity.LastLogin.ToString("yyyy-MM-dd HH:mm:ss"),
            ["@IsActive"] = entity.IsActive
        };

        return (sql, parameters);
    }

    protected override (string sql, Dictionary<string, object?> parameters) GetUpdateSqlAndParameters(User entity)
    {
        var sql = @$"
            UPDATE {_tableName} 
            SET DisplayName = @DisplayName,
                Email = @Email,
                Role = @Role,
                Department = @Department,
                BranchNumber = @BranchNumber,
                BranchName = @BranchName,
                LastLogin = @LastLogin,
                IsActive = @IsActive
            WHERE Username = @Username COLLATE NOCASE";

        var parameters = new Dictionary<string, object?>
        {
            ["@Username"] = entity.Username,
            ["@DisplayName"] = entity.DisplayName,
            ["@Email"] = entity.Email,
            ["@Role"] = (int)entity.Role,
            ["@Department"] = entity.Department,
            ["@BranchNumber"] = entity.BranchNumber,
            ["@BranchName"] = entity.BranchName,
            ["@LastLogin"] = entity.LastLogin.ToString("yyyy-MM-dd HH:mm:ss"),
            ["@IsActive"] = entity.IsActive
        };

        return (sql, parameters);
    }

    protected override int GetEntityId(User entity)
    {
        // Similar issue here - User model needs an Id property
        throw new NotImplementedException("User model needs an Id property for this operation");
    }
}