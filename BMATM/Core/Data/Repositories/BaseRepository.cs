namespace BMATM.Data.Repositories;
public interface IBaseRepository<T>
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(int id);
    Task<T?> AddAsync(T entity);
    Task<T?> UpdateAsync(T entity);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}
public abstract class BaseRepository<T> : IBaseRepository<T> where T : class
{
    protected readonly DatabaseContext _context;
    protected readonly string _tableName;

    protected BaseRepository(DatabaseContext context, string tableName)
    {
        _context = context;
        _tableName = tableName;
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        var connection = await _context.GetConnectionAsync();
        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT * FROM {_tableName} ORDER BY Id";

        using var reader = await command.ExecuteReaderAsync();
        var results = new List<T>();

        while (await reader.ReadAsync())
        {
            var entity = await MapReaderToEntityAsync(reader);
            if (entity != null)
                results.Add(entity);
        }

        return results;
    }

    public virtual async Task<T?> GetByIdAsync(int id)
    {
        var connection = await _context.GetConnectionAsync();
        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT * FROM {_tableName} WHERE Id = @Id";
        command.Parameters.AddWithValue("@Id", id);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return await MapReaderToEntityAsync(reader);
        }

        return null;
    }

    public virtual async Task<T?> AddAsync(T entity)
    {
        var connection = await _context.GetConnectionAsync();
        using var command = connection.CreateCommand();

        var (sql, parameters) = GetInsertSqlAndParameters(entity);
        command.CommandText = sql;

        foreach (var param in parameters)
        {
            command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
        }

        var id = await command.ExecuteScalarAsync();
        if (id != null && long.TryParse(id.ToString(), out var newId))
        {
            return await GetByIdAsync((int)newId);
        }

        return null;
    }

    public virtual async Task<T?> UpdateAsync(T entity)
    {
        var connection = await _context.GetConnectionAsync();
        using var command = connection.CreateCommand();

        var (sql, parameters) = GetUpdateSqlAndParameters(entity);
        command.CommandText = sql;

        foreach (var param in parameters)
        {
            command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
        }

        var rowsAffected = await command.ExecuteNonQueryAsync();
        if (rowsAffected > 0)
        {
            var id = GetEntityId(entity);
            return await GetByIdAsync(id);
        }

        return null;
    }

    public virtual async Task<bool> DeleteAsync(int id)
    {
        var connection = await _context.GetConnectionAsync();
        using var command = connection.CreateCommand();
        command.CommandText = $"DELETE FROM {_tableName} WHERE Id = @Id";
        command.Parameters.AddWithValue("@Id", id);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public virtual async Task<bool> ExistsAsync(int id)
    {
        var connection = await _context.GetConnectionAsync();
        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT COUNT(*) FROM {_tableName} WHERE Id = @Id";
        command.Parameters.AddWithValue("@Id", id);

        var count = Convert.ToInt32(await command.ExecuteScalarAsync());
        return count > 0;
    }

    // Helper methods to safely read from SqliteDataReader
    protected static string GetString(SqliteDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? string.Empty : reader.GetString(ordinal);
    }

    protected static int GetInt32(SqliteDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? 0 : reader.GetInt32(ordinal);
    }

    protected static bool GetBoolean(SqliteDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return !reader.IsDBNull(ordinal) && reader.GetBoolean(ordinal);
    }

    protected static DateTime? GetDateTime(SqliteDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        if (reader.IsDBNull(ordinal))
            return null;

        var value = reader.GetString(ordinal);
        return DateTime.TryParse(value, out var result) ? result : null;
    }

    protected static DateTime GetDateTimeOrDefault(SqliteDataReader reader, string columnName)
    {
        return GetDateTime(reader, columnName) ?? DateTime.MinValue;
    }

    // Abstract methods to be implemented by derived classes
    protected abstract Task<T?> MapReaderToEntityAsync(SqliteDataReader reader);
    protected abstract (string sql, Dictionary<string, object?> parameters) GetInsertSqlAndParameters(T entity);
    protected abstract (string sql, Dictionary<string, object?> parameters) GetUpdateSqlAndParameters(T entity);
    protected abstract int GetEntityId(T entity);

    // Helper method for JSON serialization
    protected static string SerializeToJson<TValue>(TValue value)
    {
        return JsonSerializer.Serialize(value, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    // Helper method for JSON deserialization
    protected static TValue? DeserializeFromJson<TValue>(string json)
    {
        if (string.IsNullOrEmpty(json))
            return default;

        try
        {
            return JsonSerializer.Deserialize<TValue>(json, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            });
        }
        catch
        {
            return default;
        }
    }
}