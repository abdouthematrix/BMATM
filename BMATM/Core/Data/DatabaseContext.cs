namespace BMATM.Data;

// ==================== DATABASE CONTEXT ====================
public class DatabaseContext
{
    private readonly string _connectionString;
    private static DatabaseContext _instance;
    private static readonly object _lock = new object();

    private DatabaseContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    public static DatabaseContext Instance(string connectionString = "Data Source=bmatm.db;Version=3;")
    {
        if (_instance == null)
        {
            lock (_lock)
            {
                if (_instance == null)
                    _instance = new DatabaseContext(connectionString);
            }
        }
        return _instance;
    }

    public async Task<SqliteConnection> GetConnectionAsync()
    {
        var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync().ConfigureAwait(false);
        return connection;
    }

    public async Task<T> ExecuteScalarAsync<T>(string query, List<SqliteParameter> parameters = null)
    {
        using var connection = await GetConnectionAsync().ConfigureAwait(false);
        using var command = new SqliteCommand(query, connection);

        if (parameters != null)
            command.Parameters.AddRange(parameters.ToArray());

        var result = await command.ExecuteScalarAsync().ConfigureAwait(false);
        return result == null || result == DBNull.Value ? default(T) : (T)Convert.ChangeType(result, typeof(T));
    }

    public async Task<int> ExecuteNonQueryAsync(string query, List<SqliteParameter> parameters = null)
    {
        using var connection = await GetConnectionAsync().ConfigureAwait(false);
        using var command = new SqliteCommand(query, connection);

        if (parameters != null)
            command.Parameters.AddRange(parameters.ToArray());

        return await command.ExecuteNonQueryAsync().ConfigureAwait(false);
    }

    public async Task<List<T>> ExecuteQueryAsync<T>(string query, List<SqliteParameter> parameters, Func<SqliteDataReader, T> mapper)
    {
        var results = new List<T>();
        using var connection = await GetConnectionAsync().ConfigureAwait(false);
        using var command = new SqliteCommand(query, connection);

        if (parameters != null)
            command.Parameters.AddRange(parameters.ToArray());

        using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
        while (await reader.ReadAsync().ConfigureAwait(false))
        {
            results.Add(mapper(reader));
        }
        return results;
    }
}