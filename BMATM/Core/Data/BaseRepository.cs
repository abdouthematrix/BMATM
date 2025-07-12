namespace BMATM.Data;

// ==================== BASE REPOSITORY ====================
public abstract class BaseRepository
{
    protected readonly DatabaseContext _context;
    protected readonly QueryHelper _queryHelper;
    protected readonly string TableName;

    protected BaseRepository(DatabaseContext context, string tableName)
    {
        _context = context;
        _queryHelper = new QueryHelper(tableName);
        TableName = tableName;
    }

    public abstract Task InitializeAsync();

    protected async Task<bool> TableExistsAsync()
    {
        const string query = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name=@tableName";
        var parameters = new List<SqliteParameter> { new SqliteParameter("@tableName", TableName) };
        var count = await _context.ExecuteScalarAsync<int>(query, parameters).ConfigureAwait(false);
        return count > 0;
    }
}