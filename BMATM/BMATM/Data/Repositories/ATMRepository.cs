using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using BMATM.Core.Entities;

namespace BMATM.Data.Repositories
{
    public class ATMRepository : IRepository<ATM>
    {
        private readonly SQLiteConnectionFactory _connectionFactory;

        public ATMRepository(SQLiteConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        public ATM GetById(int id)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                connection.Open();

                var query = QueryBuilder
                    .SelectById("ATMs", id)
                    .Build();

                using (var command = new SQLiteCommand(query, connection))
                {
                    var parameters = new QueryBuilder().Where("Id = ?", id).GetParameters();
                    command.Parameters.AddRange(parameters);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapFromReader(reader);
                        }
                    }
                }
            }

            return null;
        }

        public IEnumerable<ATM> GetAll()
        {
            var atms = new List<ATM>();

            using (var connection = _connectionFactory.CreateConnection())
            {
                connection.Open();

                var query = QueryBuilder
                    .SelectAll("ATMs")
                    .OrderBy("Name")
                    .Build();

                using (var command = new SQLiteCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        atms.Add(MapFromReader(reader));
                    }
                }
            }

            return atms;
        }

        public int Insert(ATM entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            using (var connection = _connectionFactory.CreateConnection())
            {
                connection.Open();

                var values = new Dictionary<string, object>
                {
                    { "SupervisorId", entity.SupervisorId },
                    { "Name", entity.Name },
                    { "Branch", entity.Branch },
                    { "GLNumber", entity.GLNumber },
                    { "ATMType", entity.ATMType },
                    { "CassetteCount", entity.CassetteCount },
                    { "IsActive", entity.IsActive ? 1 : 0 },
                    { "CreatedDate", entity.CreatedDate }
                };

                var queryBuilder = new QueryBuilder()
                    .InsertInto("ATMs")
                    .Values(values);

                using (var command = new SQLiteCommand(queryBuilder.Build(), connection))
                {
                    command.Parameters.AddRange(queryBuilder.GetParameters());
                    command.ExecuteNonQuery();

                    // Get the inserted ID
                    command.CommandText = "SELECT last_insert_rowid()";
                    command.Parameters.Clear();
                    var result = command.ExecuteScalar();

                    return Convert.ToInt32(result);
                }
            }
        }

        public bool Update(ATM entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            using (var connection = _connectionFactory.CreateConnection())
            {
                connection.Open();

                var values = new Dictionary<string, object>
                {
                    { "SupervisorId", entity.SupervisorId },
                    { "Name", entity.Name },
                    { "Branch", entity.Branch },
                    { "GLNumber", entity.GLNumber },
                    { "ATMType", entity.ATMType },
                    { "CassetteCount", entity.CassetteCount },
                    { "IsActive", entity.IsActive ? 1 : 0 }
                };

                var queryBuilder = new QueryBuilder()
                    .Update("ATMs")
                    .Set(values)
                    .Where("Id = ?", entity.Id);

                using (var command = new SQLiteCommand(queryBuilder.Build(), connection))
                {
                    command.Parameters.AddRange(queryBuilder.GetParameters());
                    int rowsAffected = command.ExecuteNonQuery();

                    return rowsAffected > 0;
                }
            }
        }

        public bool Delete(int id)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                connection.Open();

                var queryBuilder = QueryBuilder.DeleteById("ATMs", id);

                using (var command = new SQLiteCommand(queryBuilder.Build(), connection))
                {
                    command.Parameters.AddRange(queryBuilder.GetParameters());
                    int rowsAffected = command.ExecuteNonQuery();

                    return rowsAffected > 0;
                }
            }
        }

        public IEnumerable<ATM> GetPaged(int skip, int take)
        {
            var atms = new List<ATM>();

            using (var connection = _connectionFactory.CreateConnection())
            {
                connection.Open();

                var query = QueryBuilder
                    .SelectAll("ATMs")
                    .OrderBy("Name")
                    .Limit(take)
                    .Offset(skip)
                    .Build();

                using (var command = new SQLiteCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        atms.Add(MapFromReader(reader));
                    }
                }
            }

            return atms;
        }

        public int GetCount()
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                connection.Open();

                using (var command = new SQLiteCommand("SELECT COUNT(*) FROM ATMs", connection))
                {
                    var result = command.ExecuteScalar();
                    return Convert.ToInt32(result);
                }
            }
        }

        public bool Exists(int id)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                connection.Open();

                using (var command = new SQLiteCommand("SELECT COUNT(*) FROM ATMs WHERE Id = @id", connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    var result = command.ExecuteScalar();
                    return Convert.ToInt32(result) > 0;
                }
            }
        }

        // Additional methods specific to ATM entity
        public IEnumerable<ATM> GetBySupervisorId(int supervisorId)
        {
            var atms = new List<ATM>();

            using (var connection = _connectionFactory.CreateConnection())
            {
                connection.Open();

                var query = QueryBuilder
                    .SelectAll("ATMs")
                    .Where("SupervisorId = ? AND IsActive = 1", supervisorId)
                    .OrderBy("Name")
                    .Build();

                using (var command = new SQLiteCommand(query, connection))
                {
                    var parameters = new QueryBuilder().Where("SupervisorId = ?", supervisorId).GetParameters();
                    command.Parameters.AddRange(parameters);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            atms.Add(MapFromReader(reader));
                        }
                    }
                }
            }

            return atms;
        }

        public bool ExistsWithGLNumber(string glNumber, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(glNumber))
                return false;

            using (var connection = _connectionFactory.CreateConnection())
            {
                connection.Open();

                var queryBuilder = new QueryBuilder()
                    .Select("COUNT(*)")
                    .From("ATMs")
                    .Where("GLNumber = ?", glNumber);

                if (excludeId.HasValue)
                {
                    queryBuilder.And("Id != ?", excludeId.Value);
                }

                using (var command = new SQLiteCommand(queryBuilder.Build(), connection))
                {
                    command.Parameters.AddRange(queryBuilder.GetParameters());
                    var result = command.ExecuteScalar();
                    return Convert.ToInt32(result) > 0;
                }
            }
        }

        public IEnumerable<ATM> GetByBranch(string branch)
        {
            if (string.IsNullOrWhiteSpace(branch))
                return new List<ATM>();

            var atms = new List<ATM>();

            using (var connection = _connectionFactory.CreateConnection())
            {
                connection.Open();

                var query = QueryBuilder
                    .SelectAll("ATMs")
                    .Where("Branch = ? AND IsActive = 1", branch)
                    .OrderBy("Name")
                    .Build();

                using (var command = new SQLiteCommand(query, connection))
                {
                    var parameters = new QueryBuilder().Where("Branch = ?", branch).GetParameters();
                    command.Parameters.AddRange(parameters);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            atms.Add(MapFromReader(reader));
                        }
                    }
                }
            }

            return atms;
        }

        public IEnumerable<ATM> GetActiveATMs()
        {
            var atms = new List<ATM>();

            using (var connection = _connectionFactory.CreateConnection())
            {
                connection.Open();

                var query = QueryBuilder
                    .SelectAll("ATMs")
                    .Where("IsActive = 1")
                    .OrderBy("Name")
                    .Build();

                using (var command = new SQLiteCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        atms.Add(MapFromReader(reader));
                    }
                }
            }

            return atms;
        }

        public IEnumerable<string> GetDistinctBranches()
        {
            var branches = new List<string>();

            using (var connection = _connectionFactory.CreateConnection())
            {
                connection.Open();

                using (var command = new SQLiteCommand("SELECT DISTINCT Branch FROM ATMs WHERE IsActive = 1 ORDER BY Branch", connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        branches.Add(reader["Branch"].ToString());
                    }
                }
            }

            return branches;
        }

        public IEnumerable<string> GetDistinctATMTypes()
        {
            var atmTypes = new List<string>();

            using (var connection = _connectionFactory.CreateConnection())
            {
                connection.Open();

                using (var command = new SQLiteCommand("SELECT DISTINCT ATMType FROM ATMs WHERE IsActive = 1 ORDER BY ATMType", connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        atmTypes.Add(reader["ATMType"].ToString());
                    }
                }
            }

            return atmTypes;
        }

        public int GetCountBySupervisor(int supervisorId)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                connection.Open();

                using (var command = new SQLiteCommand("SELECT COUNT(*) FROM ATMs WHERE SupervisorId = @supervisorId AND IsActive = 1", connection))
                {
                    command.Parameters.AddWithValue("@supervisorId", supervisorId);
                    var result = command.ExecuteScalar();
                    return Convert.ToInt32(result);
                }
            }
        }

        // Helper method
        private ATM MapFromReader(SQLiteDataReader reader)
        {
            return new ATM
            {
                Id = Convert.ToInt32(reader["Id"]),
                SupervisorId = Convert.ToInt32(reader["SupervisorId"]),
                Name = reader["Name"].ToString(),
                Branch = reader["Branch"].ToString(),
                GLNumber = reader["GLNumber"].ToString(),
                ATMType = reader["ATMType"].ToString(),
                CassetteCount = Convert.ToInt32(reader["CassetteCount"]),
                IsActive = Convert.ToInt32(reader["IsActive"]) == 1,
                CreatedDate = Convert.ToDateTime(reader["CreatedDate"])
            };
        }
    }
}