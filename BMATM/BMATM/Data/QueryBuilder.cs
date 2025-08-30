using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;

namespace BMATM.Data
{
    public class QueryBuilder
    {
        private StringBuilder _query;
        private List<SQLiteParameter> _parameters;
        private int _parameterIndex;

        public QueryBuilder()
        {
            Clear();
        }

        public QueryBuilder Select(params string[] columns)
        {
            if (columns == null || columns.Length == 0)
            {
                _query.Append("SELECT * ");
            }
            else
            {
                _query.Append($"SELECT {string.Join(", ", columns)} ");
            }
            return this;
        }

        public QueryBuilder From(string table)
        {
            _query.Append($"FROM {table} ");
            return this;
        }

        public QueryBuilder Where(string condition, object value = null)
        {
            _query.Append($"WHERE {ProcessCondition(condition, value)} ");
            return this;
        }

        public QueryBuilder And(string condition, object value = null)
        {
            _query.Append($"AND {ProcessCondition(condition, value)} ");
            return this;
        }

        public QueryBuilder Or(string condition, object value = null)
        {
            _query.Append($"OR {ProcessCondition(condition, value)} ");
            return this;
        }

        public QueryBuilder OrderBy(string column, bool descending = false)
        {
            var direction = descending ? "DESC" : "ASC";
            _query.Append($"ORDER BY {column} {direction} ");
            return this;
        }

        public QueryBuilder Limit(int count)
        {
            _query.Append($"LIMIT {count} ");
            return this;
        }

        public QueryBuilder Offset(int count)
        {
            _query.Append($"OFFSET {count} ");
            return this;
        }

        public QueryBuilder InsertInto(string table)
        {
            _query.Append($"INSERT INTO {table} ");
            return this;
        }

        public QueryBuilder Values(Dictionary<string, object> values)
        {
            if (values == null || values.Count == 0)
                throw new ArgumentException("Values dictionary cannot be null or empty");

            var columns = string.Join(", ", values.Keys);
            var parameterNames = new List<string>();

            foreach (var kvp in values)
            {
                var paramName = AddParameter(kvp.Value);
                parameterNames.Add(paramName);
            }

            _query.Append($"({columns}) VALUES ({string.Join(", ", parameterNames)}) ");
            return this;
        }

        public QueryBuilder Update(string table)
        {
            _query.Append($"UPDATE {table} ");
            return this;
        }

        public QueryBuilder Set(Dictionary<string, object> values)
        {
            if (values == null || values.Count == 0)
                throw new ArgumentException("Values dictionary cannot be null or empty");

            var setClauses = new List<string>();
            foreach (var kvp in values)
            {
                var paramName = AddParameter(kvp.Value);
                setClauses.Add($"{kvp.Key} = {paramName}");
            }

            _query.Append($"SET {string.Join(", ", setClauses)} ");
            return this;
        }

        public QueryBuilder DeleteFrom(string table)
        {
            _query.Append($"DELETE FROM {table} ");
            return this;
        }

        public QueryBuilder Join(string table, string condition)
        {
            _query.Append($"JOIN {table} ON {condition} ");
            return this;
        }

        public QueryBuilder LeftJoin(string table, string condition)
        {
            _query.Append($"LEFT JOIN {table} ON {condition} ");
            return this;
        }

        public QueryBuilder GroupBy(params string[] columns)
        {
            if (columns != null && columns.Length > 0)
            {
                _query.Append($"GROUP BY {string.Join(", ", columns)} ");
            }
            return this;
        }

        public QueryBuilder Having(string condition, object value = null)
        {
            _query.Append($"HAVING {ProcessCondition(condition, value)} ");
            return this;
        }

        public string Build()
        {
            return _query.ToString().Trim();
        }

        public SQLiteParameter[] GetParameters()
        {
            return _parameters.ToArray();
        }

        public void Clear()
        {
            _query = new StringBuilder();
            _parameters = new List<SQLiteParameter>();
            _parameterIndex = 0;
        }

        // Helper methods for common query patterns
        public static QueryBuilder SelectAll(string table)
        {
            return new QueryBuilder().Select().From(table);
        }

        public static QueryBuilder SelectById(string table, int id)
        {
            return new QueryBuilder()
                .Select()
                .From(table)
                .Where("Id = ?", id);
        }

        public static QueryBuilder DeleteById(string table, int id)
        {
            return new QueryBuilder()
                .DeleteFrom(table)
                .Where("Id = ?", id);
        }

        // Private helper methods
        private string ProcessCondition(string condition, object value)
        {
            if (value == null)
            {
                return condition; // Assume condition is complete (e.g., "IsActive = 1")
            }

            // Replace ? with parameter placeholder
            if (condition.Contains("?"))
            {
                var paramName = AddParameter(value);
                return condition.Replace("?", paramName);
            }

            return condition;
        }

        private string AddParameter(object value)
        {
            var paramName = $"@param{_parameterIndex++}";
            _parameters.Add(new SQLiteParameter(paramName, value ?? DBNull.Value));
            return paramName;
        }
    }
}