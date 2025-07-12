namespace BMATM.Data;

// ==================== QUERY HELPER ====================
public class QueryHelper
{
    private StringBuilder _query;
    private List<SqliteParameter> _parameters;
    private string _tableName;

    public QueryHelper(string tableName)
    {
        _query = new StringBuilder();
        _parameters = new List<SqliteParameter>();
        _tableName = tableName;
    }

    public QueryHelper Select(params string[] columns)
    {
        _query.Append("SELECT ");
        _query.Append(columns.Length == 0 ? "*" : string.Join(", ", columns));
        _query.Append($" FROM {_tableName}");
        return this;
    }
    public QueryHelper Where(string column, object value, string operation = "=")
    {
        var paramName = $"@{column}_{_parameters.Count}";
        _query.Append(_query.ToString().Contains("WHERE") ? " AND " : " WHERE ");
        _query.Append($"{column} {operation} {paramName}");
        _parameters.Add(new SqliteParameter(paramName, value));
        return this;
    }

    public QueryHelper OrderBy(string column, bool ascending = true)
    {
        _query.Append($" ORDER BY {column} {(ascending ? "ASC" : "DESC")}");
        return this;
    }

    public QueryHelper Insert()
    {       
        _query.Append($"INSERT INTO {_tableName}");
        return this;
    }

    public QueryHelper Values(Dictionary<string, object> values)
    {
        var columns = new List<string>();
        var parameters = new List<string>();

        foreach (var kvp in values)
        {
            columns.Add(kvp.Key);
            var paramName = $"@{kvp.Key}";
            parameters.Add(paramName);
            _parameters.Add(new SqliteParameter(paramName, kvp.Value ?? DBNull.Value));
        }

        _query.Append($" ({string.Join(", ", columns)}) VALUES ({string.Join(", ", parameters)})");
        return this;
    }

    public QueryHelper Update()
    {
        _query.Append($"UPDATE {_tableName} SET ");
        return this;
    }

    public QueryHelper Set(Dictionary<string, object> values)
    {
        var setParts = new List<string>();
        foreach (var kvp in values)
        {
            var paramName = $"@{kvp.Key}";
            setParts.Add($"{kvp.Key} = {paramName}");
            _parameters.Add(new SqliteParameter(paramName, kvp.Value ?? DBNull.Value));
        }
        _query.Append(string.Join(", ", setParts));
        return this;
    }

    public QueryHelper Delete()
    {
        _query.Append("DELETE");
        _query.Append($" FROM {_tableName}");   
        return this;
    }

    public string Build() => _query.ToString();
    public List<SqliteParameter> GetParameters() => _parameters;

    public void Reset()
    {
        _query.Clear();
        _parameters.Clear();        
    }
}