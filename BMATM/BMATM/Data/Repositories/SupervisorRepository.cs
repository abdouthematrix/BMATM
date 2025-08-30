using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using BMATM.Core.Entities;
using BMATM.Core.Constants;

namespace BMATM.Data.Repositories
{
    public class SupervisorRepository : IRepository<Supervisor>
    {
        private readonly SQLiteConnectionFactory _connectionFactory;

        public SupervisorRepository(SQLiteConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        public Supervisor GetById(int id)
        {
            try
            {
                using (var connection = _connectionFactory.CreateConnection())
                {
                    connection.Open();

                    var query = new QueryBuilder()
                        .Select()
                        .From("Supervisors")
                        .Where("Id = ?", id)
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
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving supervisor with ID {id}", ex);
            }

            return null;
        }

        public IEnumerable<Supervisor> GetAll()
        {
            var supervisors = new List<Supervisor>();

            try
            {
                using (var connection = _connectionFactory.CreateConnection())
                {
                    connection.Open();

                    var query = new QueryBuilder()
                        .Select()
                        .From("Supervisors")
                        .OrderBy("FullName")
                        .Build();

                    using (var command = new SQLiteCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            supervisors.Add(MapFromReader(reader));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error retrieving all supervisors", ex);
            }

            return supervisors;
        }

        public int Insert(Supervisor entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            ValidateEntity(entity);

            try
            {
                using (var connection = _connectionFactory.CreateConnection())
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // Check for duplicate username
                            if (UsernameExists(entity.Username))
                            {
                                throw new InvalidOperationException($"Username '{entity.Username}' already exists");
                            }

                            var values = new Dictionary<string, object>
                            {
                                { "Username", entity.Username.Trim() },
                                { "PasswordHash", entity.PasswordHash },
                                { "FullName", entity.FullName.Trim() },
                                { "Email", string.IsNullOrWhiteSpace(entity.Email) ? null : entity.Email.Trim() },
                                { "IsActive", entity.IsActive ? 1 : 0 },
                                { "CreatedDate", entity.CreatedDate },
                                { "LastLoginDate", entity.LastLoginDate }
                            };

                            var queryBuilder = new QueryBuilder()
                                .InsertInto("Supervisors")
                                .Values(values);

                            using (var command = new SQLiteCommand(queryBuilder.Build(), connection))
                            {
                                command.Parameters.AddRange(queryBuilder.GetParameters());
                                command.ExecuteNonQuery();

                                // Get the inserted ID
                                command.CommandText = "SELECT last_insert_rowid()";
                                command.Parameters.Clear();
                                var result = command.ExecuteScalar();

                                var insertedId = Convert.ToInt32(result);
                                transaction.Commit();

                                return insertedId;
                            }
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex) when (!(ex is InvalidOperationException))
            {
                throw new InvalidOperationException("Error inserting supervisor", ex);
            }
        }

        public bool Update(Supervisor entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (entity.Id <= 0)
                throw new ArgumentException("Entity ID must be greater than zero", nameof(entity));

            ValidateEntity(entity);

            try
            {
                using (var connection = _connectionFactory.CreateConnection())
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // Check for duplicate username (excluding current record)
                            if (UsernameExists(entity.Username, entity.Id))
                            {
                                throw new InvalidOperationException($"Username '{entity.Username}' already exists");
                            }

                            var values = new Dictionary<string, object>
                            {
                                { "Username", entity.Username.Trim() },
                                { "PasswordHash", entity.PasswordHash },
                                { "FullName", entity.FullName.Trim() },
                                { "Email", string.IsNullOrWhiteSpace(entity.Email) ? null : entity.Email.Trim() },
                                { "IsActive", entity.IsActive ? 1 : 0 },
                                { "LastLoginDate", entity.LastLoginDate }
                            };

                            var queryBuilder = new QueryBuilder()
                                .Update("Supervisors")
                                .Set(values)
                                .Where("Id = ?", entity.Id);

                            using (var command = new SQLiteCommand(queryBuilder.Build(), connection))
                            {
                                command.Parameters.AddRange(queryBuilder.GetParameters());
                                int rowsAffected = command.ExecuteNonQuery();

                                transaction.Commit();
                                return rowsAffected > 0;
                            }
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex) when (!(ex is InvalidOperationException) && !(ex is ArgumentException))
            {
                throw new InvalidOperationException($"Error updating supervisor with ID {entity.Id}", ex);
            }
        }

        public bool Delete(int id)
        {
            if (id <= 0)
                throw new ArgumentException("ID must be greater than zero", nameof(id));

            try
            {
                using (var connection = _connectionFactory.CreateConnection())
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // Check if supervisor has ATMs assigned
                            using (var checkCommand = new SQLiteCommand("SELECT COUNT(*) FROM ATMs WHERE SupervisorId = @id AND IsActive = 1", connection))
                            {
                                checkCommand.Parameters.AddWithValue("@id", id);
                                var atmCount = Convert.ToInt32(checkCommand.ExecuteScalar());

                                if (atmCount > 0)
                                {
                                    throw new InvalidOperationException($"Cannot delete supervisor. {atmCount} active ATMs are still assigned to this supervisor");
                                }
                            }

                            var queryBuilder = new QueryBuilder()
                                .DeleteFrom("Supervisors")
                                .Where("Id = ?", id);

                            using (var command = new SQLiteCommand(queryBuilder.Build(), connection))
                            {
                                command.Parameters.AddRange(queryBuilder.GetParameters());
                                int rowsAffected = command.ExecuteNonQuery();

                                transaction.Commit();
                                return rowsAffected > 0;
                            }
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex) when (!(ex is InvalidOperationException) && !(ex is ArgumentException))
            {
                throw new InvalidOperationException($"Error deleting supervisor with ID {id}", ex);
            }
        }

        public IEnumerable<Supervisor> GetPaged(int skip, int take)
        {
            if (skip < 0) throw new ArgumentException("Skip must be non-negative", nameof(skip));
            if (take <= 0) throw new ArgumentException("Take must be positive", nameof(take));

            var supervisors = new List<Supervisor>();

            try
            {
                using (var connection = _connectionFactory.CreateConnection())
                {
                    connection.Open();

                    var query = new QueryBuilder()
                        .Select()
                        .From("Supervisors")
                        .OrderBy("FullName")
                        .Limit(take)
                        .Offset(skip)
                        .Build();

                    using (var command = new SQLiteCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            supervisors.Add(MapFromReader(reader));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving paged supervisors (skip: {skip}, take: {take})", ex);
            }

            return supervisors;
        }

        public int GetCount()
        {
            try
            {
                using (var connection = _connectionFactory.CreateConnection())
                {
                    connection.Open();

                    using (var command = new SQLiteCommand("SELECT COUNT(*) FROM Supervisors", connection))
                    {
                        var result = command.ExecuteScalar();
                        return Convert.ToInt32(result);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error retrieving supervisor count", ex);
            }
        }

        public bool Exists(int id)
        {
            if (id <= 0) return false;

            try
            {
                using (var connection = _connectionFactory.CreateConnection())
                {
                    connection.Open();

                    using (var command = new SQLiteCommand("SELECT COUNT(*) FROM Supervisors WHERE Id = @id", connection))
                    {
                        command.Parameters.AddWithValue("@id", id);
                        var result = command.ExecuteScalar();
                        return Convert.ToInt32(result) > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error checking existence of supervisor with ID {id}", ex);
            }
        }

        // Additional methods specific to Supervisor entity
        public Supervisor GetByUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return null;

            try
            {
                using (var connection = _connectionFactory.CreateConnection())
                {
                    connection.Open();

                    var query = new QueryBuilder()
                        .Select()
                        .From("Supervisors")
                        .Where("LOWER(Username) = LOWER(?) AND IsActive = 1", username.Trim())
                        .Build();

                    using (var command = new SQLiteCommand(query, connection))
                    {
                        var parameters = new QueryBuilder().Where("LOWER(Username) = LOWER(?)", username.Trim()).GetParameters();
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
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving supervisor by username '{username}'", ex);
            }

            return null;
        }

        public bool ValidateCredentials(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return false;

            try
            {
                var supervisor = GetByUsername(username);
                if (supervisor == null || !supervisor.IsActive)
                    return false;

                var passwordHash = HashPassword(password);
                return supervisor.PasswordHash == passwordHash;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error validating credentials for username '{username}'", ex);
            }
        }

        public void UpdateLastLoginDate(int supervisorId)
        {
            if (supervisorId <= 0)
                throw new ArgumentException("Supervisor ID must be greater than zero", nameof(supervisorId));

            try
            {
                using (var connection = _connectionFactory.CreateConnection())
                {
                    connection.Open();

                    var values = new Dictionary<string, object>
                    {
                        { "LastLoginDate", DateTime.Now }
                    };

                    var queryBuilder = new QueryBuilder()
                        .Update("Supervisors")
                        .Set(values)
                        .Where("Id = ?", supervisorId);

                    using (var command = new SQLiteCommand(queryBuilder.Build(), connection))
                    {
                        command.Parameters.AddRange(queryBuilder.GetParameters());
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error updating last login date for supervisor ID {supervisorId}", ex);
            }
        }

        public bool UsernameExists(string username, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(username))
                return false;

            try
            {
                using (var connection = _connectionFactory.CreateConnection())
                {
                    connection.Open();

                    var queryBuilder = new QueryBuilder()
                        .Select("COUNT(*)")
                        .From("Supervisors")
                        .Where("LOWER(Username) = LOWER(?)", username.Trim());

                    if (excludeId.HasValue && excludeId.Value > 0)
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
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error checking username existence for '{username}'", ex);
            }
        }

        public IEnumerable<Supervisor> GetActiveSupervisors()
        {
            var supervisors = new List<Supervisor>();

            try
            {
                using (var connection = _connectionFactory.CreateConnection())
                {
                    connection.Open();

                    var query = new QueryBuilder()
                        .Select()
                        .From("Supervisors")
                        .Where("IsActive = 1")
                        .OrderBy("FullName")
                        .Build();

                    using (var command = new SQLiteCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            supervisors.Add(MapFromReader(reader));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error retrieving active supervisors", ex);
            }

            return supervisors;
        }

        public bool DeactivateSupervisor(int supervisorId)
        {
            if (supervisorId <= 0)
                throw new ArgumentException("Supervisor ID must be greater than zero", nameof(supervisorId));

            try
            {
                using (var connection = _connectionFactory.CreateConnection())
                {
                    connection.Open();

                    var values = new Dictionary<string, object>
                    {
                        { "IsActive", 0 }
                    };

                    var queryBuilder = new QueryBuilder()
                        .Update("Supervisors")
                        .Set(values)
                        .Where("Id = ?", supervisorId);

                    using (var command = new SQLiteCommand(queryBuilder.Build(), connection))
                    {
                        command.Parameters.AddRange(queryBuilder.GetParameters());
                        int rowsAffected = command.ExecuteNonQuery();

                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error deactivating supervisor with ID {supervisorId}", ex);
            }
        }

        public bool ChangePassword(int supervisorId, string newPassword)
        {
            if (supervisorId <= 0)
                throw new ArgumentException("Supervisor ID must be greater than zero", nameof(supervisorId));

            if (string.IsNullOrWhiteSpace(newPassword))
                throw new ArgumentException("New password cannot be null or empty", nameof(newPassword));

            if (newPassword.Length < AppConstants.PASSWORD_MIN_LENGTH)
                throw new ArgumentException($"Password must be at least {AppConstants.PASSWORD_MIN_LENGTH} characters long", nameof(newPassword));

            try
            {
                using (var connection = _connectionFactory.CreateConnection())
                {
                    connection.Open();

                    var values = new Dictionary<string, object>
                    {
                        { "PasswordHash", HashPassword(newPassword) }
                    };

                    var queryBuilder = new QueryBuilder()
                        .Update("Supervisors")
                        .Set(values)
                        .Where("Id = ?", supervisorId);

                    using (var command = new SQLiteCommand(queryBuilder.Build(), connection))
                    {
                        command.Parameters.AddRange(queryBuilder.GetParameters());
                        int rowsAffected = command.ExecuteNonQuery();

                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex) when (!(ex is ArgumentException))
            {
                throw new InvalidOperationException($"Error changing password for supervisor with ID {supervisorId}", ex);
            }
        }

        public IEnumerable<Supervisor> SearchByName(string nameQuery)
        {
            if (string.IsNullOrWhiteSpace(nameQuery))
                return new List<Supervisor>();

            var supervisors = new List<Supervisor>();

            try
            {
                using (var connection = _connectionFactory.CreateConnection())
                {
                    connection.Open();

                    var searchTerm = $"%{nameQuery.Trim()}%";
                    var query = new QueryBuilder()
                        .Select()
                        .From("Supervisors")
                        .Where("(LOWER(FullName) LIKE LOWER(?) OR LOWER(Username) LIKE LOWER(?)) AND IsActive = 1", searchTerm)
                        .OrderBy("FullName")
                        .Build();

                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@term1", searchTerm);
                        command.Parameters.AddWithValue("@term2", searchTerm);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                supervisors.Add(MapFromReader(reader));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error searching supervisors by name '{nameQuery}'", ex);
            }

            return supervisors;
        }

        // Helper methods
        private Supervisor MapFromReader(SQLiteDataReader reader)
        {
            try
            {
                return new Supervisor
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    Username = reader["Username"]?.ToString(),
                    PasswordHash = reader["PasswordHash"]?.ToString(),
                    FullName = reader["FullName"]?.ToString(),
                    Email = reader["Email"] == DBNull.Value ? null : reader["Email"].ToString(),
                    IsActive = Convert.ToInt32(reader["IsActive"]) == 1,
                    CreatedDate = Convert.ToDateTime(reader["CreatedDate"]),
                    LastLoginDate = reader["LastLoginDate"] == DBNull.Value
                        ? (DateTime?)null
                        : Convert.ToDateTime(reader["LastLoginDate"])
                };

            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error mapping supervisor from database reader", ex);
            }
        }

        private void ValidateEntity(Supervisor entity)
        {
            if (string.IsNullOrWhiteSpace(entity.Username))
                throw new ArgumentException("Username cannot be null or empty");

            if (entity.Username.Length > 50)
                throw new ArgumentException("Username cannot exceed 50 characters");

            if (string.IsNullOrWhiteSpace(entity.PasswordHash))
                throw new ArgumentException("Password hash cannot be null or empty");

            if (string.IsNullOrWhiteSpace(entity.FullName))
                throw new ArgumentException("Full name cannot be null or empty");

            if (entity.FullName.Length > 100)
                throw new ArgumentException("Full name cannot exceed 100 characters");

            if (!string.IsNullOrWhiteSpace(entity.Email) && entity.Email.Length > 100)
                throw new ArgumentException("Email cannot exceed 100 characters");

            if (!string.IsNullOrWhiteSpace(entity.Email) && !IsValidEmail(entity.Email))
                throw new ArgumentException("Email format is invalid");
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public static string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be null or empty", nameof(password));

            try
            {
                using (var sha256 = SHA256.Create())
                {
                    var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                    return Convert.ToBase64String(hashedBytes);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error hashing password", ex);
            }
        }
    }
}