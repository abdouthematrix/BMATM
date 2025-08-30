using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using BMATM.Core.Entities;
using BMATM.Core.Constants;

namespace BMATM.Data.Repositories
{
    public class TransactionRepository : IRepository<ATMTransaction>
    {
        private readonly SQLiteConnectionFactory _connectionFactory;

        public TransactionRepository(SQLiteConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        public ATMTransaction GetById(int id)
        {
            try
            {
                using (var connection = _connectionFactory.CreateConnection())
                {
                    connection.Open();

                    var query = new QueryBuilder()
                        .Select()
                        .From("ATMTransactions")
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
                throw new InvalidOperationException($"Error retrieving transaction with ID {id}", ex);
            }

            return null;
        }

        public IEnumerable<ATMTransaction> GetAll()
        {
            var transactions = new List<ATMTransaction>();

            try
            {
                using (var connection = _connectionFactory.CreateConnection())
                {
                    connection.Open();

                    var query = new QueryBuilder()
                        .Select()
                        .From("ATMTransactions")
                        .OrderBy("TransactionDate", true) // Most recent first
                        .Build();

                    using (var command = new SQLiteCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            transactions.Add(MapFromReader(reader));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error retrieving all transactions", ex);
            }

            return transactions;
        }

        public int Insert(ATMTransaction entity)
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
                            // Check if transaction already exists for this ATM and date
                            if (HasTransactionForDate(entity.ATMId, entity.TransactionDate))
                            {
                                throw new InvalidOperationException($"A transaction already exists for ATM {entity.ATMId} on {entity.TransactionDate:yyyy-MM-dd}");
                            }

                            // Verify ATM exists and is active
                            if (!VerifyATMExists(entity.ATMId, connection))
                            {
                                throw new InvalidOperationException($"ATM with ID {entity.ATMId} does not exist or is inactive");
                            }

                            // Calculate variance if not provided
                            if (!entity.Variance.HasValue && entity.EndingCash.HasValue)
                            {
                                entity.Variance = entity.CalculatedVariance;
                            }

                            // Determine reconciliation status if not provided
                            if (string.IsNullOrWhiteSpace(entity.ReconciliationStatus))
                            {
                                entity.ReconciliationStatus = DetermineReconciliationStatus(entity.Variance);
                            }

                            var values = new Dictionary<string, object>
                            {
                                { "ATMId", entity.ATMId },
                                { "TransactionDate", entity.TransactionDate.Date },
                                { "BeginningCash", entity.BeginningCash },
                                { "AddedCash", entity.AddedCash },
                                { "RecycledCash", entity.RecycledCash },
                                { "EndingCash", entity.EndingCash },
                                { "DepositedCash", entity.DepositedCash },
                                { "GLBalance", entity.GLBalance },
                                { "IsReconciled", entity.IsReconciled ? 1 : 0 },
                                { "ReconciliationStatus", entity.ReconciliationStatus ?? AppConstants.STATUS_PENDING },
                                { "Variance", entity.Variance },
                                { "Notes", string.IsNullOrWhiteSpace(entity.Notes) ? null : entity.Notes.Trim() },
                                { "CreatedDate", entity.CreatedDate }
                            };

                            var queryBuilder = new QueryBuilder()
                                .InsertInto("ATMTransactions")
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
                throw new InvalidOperationException("Error inserting transaction", ex);
            }
        }

        public bool Update(ATMTransaction entity)
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
                            // Verify ATM exists and is active
                            if (!VerifyATMExists(entity.ATMId, connection))
                            {
                                throw new InvalidOperationException($"ATM with ID {entity.ATMId} does not exist or is inactive");
                            }

                            // Check for duplicate transaction (excluding current record)
                            if (HasTransactionForDate(entity.ATMId, entity.TransactionDate, entity.Id))
                            {
                                throw new InvalidOperationException($"Another transaction already exists for ATM {entity.ATMId} on {entity.TransactionDate:yyyy-MM-dd}");
                            }

                            // Calculate variance if not provided
                            if (!entity.Variance.HasValue && entity.EndingCash.HasValue)
                            {
                                entity.Variance = entity.CalculatedVariance;
                            }

                            // Update reconciliation status based on variance
                            if (!string.IsNullOrWhiteSpace(entity.ReconciliationStatus) && entity.ReconciliationStatus != AppConstants.STATUS_PENDING)
                            {
                                entity.IsReconciled = true;
                            }

                            var values = new Dictionary<string, object>
                            {
                                { "ATMId", entity.ATMId },
                                { "TransactionDate", entity.TransactionDate.Date },
                                { "BeginningCash", entity.BeginningCash },
                                { "AddedCash", entity.AddedCash },
                                { "RecycledCash", entity.RecycledCash },
                                { "EndingCash", entity.EndingCash },
                                { "DepositedCash", entity.DepositedCash },
                                { "GLBalance", entity.GLBalance },
                                { "IsReconciled", entity.IsReconciled ? 1 : 0 },
                                { "ReconciliationStatus", entity.ReconciliationStatus ?? AppConstants.STATUS_PENDING },
                                { "Variance", entity.Variance },
                                { "Notes", string.IsNullOrWhiteSpace(entity.Notes) ? null : entity.Notes.Trim() }
                            };

                            var queryBuilder = new QueryBuilder()
                                .Update("ATMTransactions")
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
                throw new InvalidOperationException($"Error updating transaction with ID {entity.Id}", ex);
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
                            // Check if transaction exists and get its details for validation
                            var existingTransaction = GetById(id);
                            if (existingTransaction == null)
                            {
                                return false; // Transaction doesn't exist
                            }

                            // Optional: Add business rules for deletion
                            // For example, prevent deletion of reconciled transactions
                            if (existingTransaction.IsReconciled)
                            {
                                throw new InvalidOperationException("Cannot delete reconciled transactions");
                            }

                            var queryBuilder = new QueryBuilder()
                                .DeleteFrom("ATMTransactions")
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
                throw new InvalidOperationException($"Error deleting transaction with ID {id}", ex);
            }
        }

        public IEnumerable<ATMTransaction> GetPaged(int skip, int take)
        {
            if (skip < 0) throw new ArgumentException("Skip must be non-negative", nameof(skip));
            if (take <= 0) throw new ArgumentException("Take must be positive", nameof(take));

            var transactions = new List<ATMTransaction>();

            try
            {
                using (var connection = _connectionFactory.CreateConnection())
                {
                    connection.Open();

                    var query = new QueryBuilder()
                        .Select()
                        .From("ATMTransactions")
                        .OrderBy("TransactionDate", true)
                        .Limit(take)
                        .Offset(skip)
                        .Build();

                    using (var command = new SQLiteCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            transactions.Add(MapFromReader(reader));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving paged transactions (skip: {skip}, take: {take})", ex);
            }

            return transactions;
        }

        public int GetCount()
        {
            try
            {
                using (var connection = _connectionFactory.CreateConnection())
                {
                    connection.Open();

                    using (var command = new SQLiteCommand("SELECT COUNT(*) FROM ATMTransactions", connection))
                    {
                        var result = command.ExecuteScalar();
                        return Convert.ToInt32(result);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error retrieving transaction count", ex);
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

                    using (var command = new SQLiteCommand("SELECT COUNT(*) FROM ATMTransactions WHERE Id = @id", connection))
                    {
                        command.Parameters.AddWithValue("@id", id);
                        var result = command.ExecuteScalar();
                        return Convert.ToInt32(result) > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error checking existence of transaction with ID {id}", ex);
            }
        }

        // Additional methods specific to ATMTransaction entity
        public IEnumerable<ATMTransaction> GetByATMId(int atmId)
        {
            if (atmId <= 0)
                throw new ArgumentException("ATM ID must be greater than zero", nameof(atmId));

            var transactions = new List<ATMTransaction>();

            try
            {
                using (var connection = _connectionFactory.CreateConnection())
                {
                    connection.Open();

                    var query = new QueryBuilder()
                        .Select()
                        .From("ATMTransactions")
                        .Where("ATMId = ?", atmId)
                        .OrderBy("TransactionDate", true)
                        .Build();

                    using (var command = new SQLiteCommand(query, connection))
                    {
                        var parameters = new QueryBuilder().Where("ATMId = ?", atmId).GetParameters();
                        command.Parameters.AddRange(parameters);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                transactions.Add(MapFromReader(reader));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving transactions for ATM ID {atmId}", ex);
            }

            return transactions;
        }

        public ATMTransaction GetByATMAndDate(int atmId, DateTime transactionDate)
        {
            if (atmId <= 0)
                throw new ArgumentException("ATM ID must be greater than zero", nameof(atmId));

            try
            {
                using (var connection = _connectionFactory.CreateConnection())
                {
                    connection.Open();

                    using (var command = new SQLiteCommand("SELECT * FROM ATMTransactions WHERE ATMId = @atmId AND DATE(TransactionDate) = DATE(@transactionDate)", connection))
                    {
                        command.Parameters.AddWithValue("@atmId", atmId);
                        command.Parameters.AddWithValue("@transactionDate", transactionDate.Date);

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
                throw new InvalidOperationException($"Error retrieving transaction for ATM {atmId} on date {transactionDate:yyyy-MM-dd}", ex);
            }

            return null;
        }

        public IEnumerable<ATMTransaction> GetByDateRange(DateTime startDate, DateTime endDate)
        {
            if (startDate > endDate)
                throw new ArgumentException("Start date must be less than or equal to end date");

            var transactions = new List<ATMTransaction>();

            try
            {
                using (var connection = _connectionFactory.CreateConnection())
                {
                    connection.Open();

                    using (var command = new SQLiteCommand("SELECT * FROM ATMTransactions WHERE DATE(TransactionDate) >= DATE(@startDate) AND DATE(TransactionDate) <= DATE(@endDate) ORDER BY TransactionDate DESC", connection))
                    {
                        command.Parameters.AddWithValue("@startDate", startDate.Date);
                        command.Parameters.AddWithValue("@endDate", endDate.Date);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                transactions.Add(MapFromReader(reader));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving transactions between {startDate:yyyy-MM-dd} and {endDate:yyyy-MM-dd}", ex);
            }

            return transactions;
        }

        public IEnumerable<ATMTransaction> GetUnreconciledTransactions()
        {
            var transactions = new List<ATMTransaction>();

            try
            {
                using (var connection = _connectionFactory.CreateConnection())
                {
                    connection.Open();

                    var query = new QueryBuilder()
                        .Select()
                        .From("ATMTransactions")
                        .Where("IsReconciled = 0 OR ReconciliationStatus = ?", AppConstants.STATUS_PENDING)
                        .OrderBy("TransactionDate", true)
                        .Build();

                    using (var command = new SQLiteCommand(query, connection))
                    {
                        var parameters = new QueryBuilder().Where("ReconciliationStatus = ?", AppConstants.STATUS_PENDING).GetParameters();
                        command.Parameters.AddRange(parameters);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                transactions.Add(MapFromReader(reader));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error retrieving unreconciled transactions", ex);
            }

            return transactions;
        }

        public IEnumerable<ATMTransaction> GetByReconciliationStatus(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return new List<ATMTransaction>();

            var transactions = new List<ATMTransaction>();

            try
            {
                using (var connection = _connectionFactory.CreateConnection())
                {
                    connection.Open();

                    var query = new QueryBuilder()
                        .Select()
                        .From("ATMTransactions")
                        .Where("ReconciliationStatus = ?", status.Trim())
                        .OrderBy("TransactionDate", true)
                        .Build();

                    using (var command = new SQLiteCommand(query, connection))
                    {
                        var parameters = new QueryBuilder().Where("ReconciliationStatus = ?", status.Trim()).GetParameters();
                        command.Parameters.AddRange(parameters);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                transactions.Add(MapFromReader(reader));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving transactions with status '{status}'", ex);
            }

            return transactions;
        }

        public decimal GetTotalVarianceByATM(int atmId, DateTime? startDate = null, DateTime? endDate = null)
        {
            if (atmId <= 0)
                throw new ArgumentException("ATM ID must be greater than zero", nameof(atmId));

            try
            {
                using (var connection = _connectionFactory.CreateConnection())
                {
                    connection.Open();

                    var queryBuilder = new QueryBuilder()
                        .Select("SUM(COALESCE(Variance, 0))")
                        .From("ATMTransactions")
                        .Where("ATMId = ?", atmId);

                    if (startDate.HasValue)
                    {
                        queryBuilder.And("DATE(TransactionDate) >= DATE(?)", startDate.Value.Date);
                    }

                    if (endDate.HasValue)
                    {
                        queryBuilder.And("DATE(TransactionDate) <= DATE(?)", endDate.Value.Date);
                    }

                    using (var command = new SQLiteCommand(queryBuilder.Build(), connection))
                    {
                        command.Parameters.AddRange(queryBuilder.GetParameters());
                        var result = command.ExecuteScalar();
                        return result == DBNull.Value ? 0 : Convert.ToDecimal(result);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error calculating total variance for ATM {atmId}", ex);
            }
        }

        public int GetReconciliationCountByStatus(string status, DateTime? startDate = null, DateTime? endDate = null)
        {
            if (string.IsNullOrWhiteSpace(status))
                return 0;

            try
            {
                using (var connection = _connectionFactory.CreateConnection())
                {
                    connection.Open();

                    var queryBuilder = new QueryBuilder()
                        .Select("COUNT(*)")
                        .From("ATMTransactions")
                        .Where("ReconciliationStatus = ?", status.Trim());

                    if (startDate.HasValue)
                    {
                        queryBuilder.And("DATE(TransactionDate) >= DATE(?)", startDate.Value.Date);
                    }

                    if (endDate.HasValue)
                    {
                        queryBuilder.And("DATE(TransactionDate) <= DATE(?)", endDate.Value.Date);
                    }

                    using (var command = new SQLiteCommand(queryBuilder.Build(), connection))
                    {
                        command.Parameters.AddRange(queryBuilder.GetParameters());
                        var result = command.ExecuteScalar();
                        return Convert.ToInt32(result);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error counting transactions with status '{status}'", ex);
            }
        }

        public IEnumerable<ATMTransaction> GetRecentTransactions(int count = 10)
        {
            if (count <= 0)
                throw new ArgumentException("Count must be positive", nameof(count));

            var transactions = new List<ATMTransaction>();

            try
            {
                using (var connection = _connectionFactory.CreateConnection())
                {
                    connection.Open();

                    var query = new QueryBuilder()
                        .Select()
                        .From("ATMTransactions")
                        .OrderBy("CreatedDate", true)
                        .Limit(count)
                        .Build();

                    using (var command = new SQLiteCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            transactions.Add(MapFromReader(reader));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving {count} recent transactions", ex);
            }

            return transactions;
        }

        public bool HasTransactionForDate(int atmId, DateTime date, int? excludeTransactionId = null)
        {
            if (atmId <= 0)
                throw new ArgumentException("ATM ID must be greater than zero", nameof(atmId));

            try
            {
                using (var connection = _connectionFactory.CreateConnection())
                {
                    connection.Open();

                    var query = "SELECT COUNT(*) FROM ATMTransactions WHERE ATMId = @atmId AND DATE(TransactionDate) = DATE(@date)";
                    if (excludeTransactionId.HasValue && excludeTransactionId.Value > 0)
                    {
                        query += " AND Id != @excludeId";
                    }

                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@atmId", atmId);
                        command.Parameters.AddWithValue("@date", date.Date);
                        if (excludeTransactionId.HasValue && excludeTransactionId.Value > 0)
                        {
                            command.Parameters.AddWithValue("@excludeId", excludeTransactionId.Value);
                        }

                        var result = command.ExecuteScalar();
                        return Convert.ToInt32(result) > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error checking transaction existence for ATM {atmId} on {date:yyyy-MM-dd}", ex);
            }
        }

        public void UpdateReconciliationStatus(int transactionId, string status, decimal? variance = null, string notes = null)
        {
            if (transactionId <= 0)
                throw new ArgumentException("Transaction ID must be greater than zero", nameof(transactionId));

            if (string.IsNullOrWhiteSpace(status))
                throw new ArgumentException("Status cannot be null or empty", nameof(status));

            try
            {
                using (var connection = _connectionFactory.CreateConnection())
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            var values = new Dictionary<string, object>
                            {
                                { "ReconciliationStatus", status.Trim() },
                                { "IsReconciled", status.Trim() != AppConstants.STATUS_PENDING ? 1 : 0 }
                            };

                            if (variance.HasValue)
                                values.Add("Variance", variance.Value);

                            if (!string.IsNullOrWhiteSpace(notes))
                                values.Add("Notes", notes.Trim());

                            var queryBuilder = new QueryBuilder()
                                .Update("ATMTransactions")
                                .Set(values)
                                .Where("Id = ?", transactionId);

                            using (var command = new SQLiteCommand(queryBuilder.Build(), connection))
                            {
                                command.Parameters.AddRange(queryBuilder.GetParameters());
                                int rowsAffected = command.ExecuteNonQuery();

                                if (rowsAffected == 0)
                                {
                                    throw new InvalidOperationException($"Transaction with ID {transactionId} not found");
                                }

                                transaction.Commit();
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
                throw new InvalidOperationException($"Error updating reconciliation status for transaction {transactionId}", ex);
            }
        }

        public IEnumerable<ATMTransaction> GetTransactionsWithVariance(decimal minVariance)
        {
            var transactions = new List<ATMTransaction>();

            try
            {
                using (var connection = _connectionFactory.CreateConnection())
                {
                    connection.Open();

                    var query = new QueryBuilder()
                        .Select()
                        .From("ATMTransactions")
                        .Where("ABS(COALESCE(Variance, 0)) >= ?", Math.Abs(minVariance))
                        .OrderBy("TransactionDate", true)
                        .Build();

                    using (var command = new SQLiteCommand(query, connection))
                    {
                        var parameters = new QueryBuilder().Where("ABS(COALESCE(Variance, 0)) >= ?", Math.Abs(minVariance)).GetParameters();
                        command.Parameters.AddRange(parameters);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                transactions.Add(MapFromReader(reader));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving transactions with variance >= {minVariance}", ex);
            }

            return transactions;
        }

        public IEnumerable<ATMTransaction> GetTransactionsByATMAndDateRange(int atmId, DateTime startDate, DateTime endDate)
        {
            if (atmId <= 0)
                throw new ArgumentException("ATM ID must be greater than zero", nameof(atmId));

            if (startDate > endDate)
                throw new ArgumentException("Start date must be less than or equal to end date");

            var transactions = new List<ATMTransaction>();

            try
            {
                using (var connection = _connectionFactory.CreateConnection())
                {
                    connection.Open();

                    using (var command = new SQLiteCommand("SELECT * FROM ATMTransactions WHERE ATMId = @atmId AND DATE(TransactionDate) >= DATE(@startDate) AND DATE(TransactionDate) <= DATE(@endDate) ORDER BY TransactionDate DESC", connection))
                    {
                        command.Parameters.AddWithValue("@atmId", atmId);
                        command.Parameters.AddWithValue("@startDate", startDate.Date);
                        command.Parameters.AddWithValue("@endDate", endDate.Date);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                transactions.Add(MapFromReader(reader));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving transactions for ATM {atmId} between {startDate:yyyy-MM-dd} and {endDate:yyyy-MM-dd}", ex);
            }

            return transactions;
        }

        // Helper methods
        private ATMTransaction MapFromReader(SQLiteDataReader reader)
        {
            try
            {
                return new ATMTransaction
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    ATMId = Convert.ToInt32(reader["ATMId"]),
                    TransactionDate = Convert.ToDateTime(reader["TransactionDate"]),
                    BeginningCash = reader["BeginningCash"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["BeginningCash"]),
                    AddedCash = reader["AddedCash"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["AddedCash"]),
                    RecycledCash = reader["RecycledCash"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["RecycledCash"]),
                    EndingCash = reader["EndingCash"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["EndingCash"]),
                    DepositedCash = reader["DepositedCash"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["DepositedCash"]),
                    GLBalance = reader["GLBalance"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["GLBalance"]),
                    IsReconciled = Convert.ToInt32(reader["IsReconciled"]) == 1,
                    ReconciliationStatus = reader["ReconciliationStatus"] == DBNull.Value
                              ? AppConstants.STATUS_PENDING
                              : reader["ReconciliationStatus"].ToString(),
                    Variance = reader["Variance"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["Variance"]),
                    Notes = reader["Notes"] == DBNull.Value ? null : reader["Notes"].ToString(),
                    CreatedDate = Convert.ToDateTime(reader["CreatedDate"])
                };

            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error mapping transaction from database reader", ex);
            }
        }

        private void ValidateEntity(ATMTransaction entity)
        {
            if (entity.ATMId <= 0)
                throw new ArgumentException("ATM ID must be greater than zero");

            if (entity.TransactionDate == default(DateTime))
                throw new ArgumentException("Transaction date cannot be default value");

            if (entity.TransactionDate > DateTime.Today.AddDays(1))
                throw new ArgumentException("Transaction date cannot be in the future");

            if (entity.BeginningCash.HasValue && entity.BeginningCash.Value < 0)
                throw new ArgumentException("Beginning cash cannot be negative");

            if (entity.AddedCash.HasValue && entity.AddedCash.Value < 0)
                throw new ArgumentException("Added cash cannot be negative");

            if (entity.RecycledCash.HasValue && entity.RecycledCash.Value < 0)
                throw new ArgumentException("Recycled cash cannot be negative");

            if (entity.EndingCash.HasValue && entity.EndingCash.Value < 0)
                throw new ArgumentException("Ending cash cannot be negative");

            if (entity.DepositedCash.HasValue && entity.DepositedCash.Value < 0)
                throw new ArgumentException("Deposited cash cannot be negative");

            if (!string.IsNullOrWhiteSpace(entity.Notes) && entity.Notes.Length > 1000)
                throw new ArgumentException("Notes cannot exceed 1000 characters");

            if (!string.IsNullOrWhiteSpace(entity.ReconciliationStatus))
            {
                var validStatuses = new[] { AppConstants.STATUS_PENDING, AppConstants.STATUS_BALANCED, AppConstants.STATUS_SHORTAGE, AppConstants.STATUS_OVER };
                if (!validStatuses.Contains(entity.ReconciliationStatus))
                    throw new ArgumentException($"Invalid reconciliation status. Must be one of: {string.Join(", ", validStatuses)}");
            }
        }

        private bool VerifyATMExists(int atmId, SQLiteConnection connection)
        {
            using (var command = new SQLiteCommand("SELECT COUNT(*) FROM ATMs WHERE Id = @id AND IsActive = 1", connection))
            {
                command.Parameters.AddWithValue("@id", atmId);
                var result = command.ExecuteScalar();
                return Convert.ToInt32(result) > 0;
            }
        }

        private string DetermineReconciliationStatus(decimal? variance)
        {
            if (!variance.HasValue) return AppConstants.STATUS_PENDING;

            var absVariance = Math.Abs(variance.Value);
            if (absVariance <= AppConstants.DEFAULT_CASH_TOLERANCE)
                return AppConstants.STATUS_BALANCED;
            else if (variance.Value < 0)
                return AppConstants.STATUS_SHORTAGE;
            else
                return AppConstants.STATUS_OVER;
        }
    }
}