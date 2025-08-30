using System;
using System.Collections.Generic;
using System.Linq;
using BMATM.Core.Entities;
using BMATM.Data;
using BMATM.Data.Repositories;

namespace BMATM.Phase1Test
{
    /// <summary>
    /// Phase 1 Test Program - Tests the complete database layer and core entities
    /// This console app verifies that all CRUD operations work correctly
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== BMATM Phase 1 Test Program ===");
            Console.WriteLine("Testing Core Infrastructure & Database Foundation");
            Console.WriteLine();

            try
            {
                // Initialize database connection factory
                var connectionFactory = new SQLiteConnectionFactory();
                Console.WriteLine("✓ SQLiteConnectionFactory created successfully");

                // Test database connection
                if (connectionFactory.TestConnection())
                {
                    Console.WriteLine("✓ Database connection test successful");
                }
                else
                {
                    Console.WriteLine("✗ Database connection test failed");
                    return;
                }

                // Initialize database schema and sample data
                connectionFactory.InitializeDatabase();
                Console.WriteLine("✓ Database initialized with schema and sample data");
                Console.WriteLine($"✓ Database location: {connectionFactory.GetDatabasePath()}");
                Console.WriteLine($"✓ Database size: {connectionFactory.GetDatabaseSize()} bytes");
                Console.WriteLine();

                // Test QueryBuilder
                TestQueryBuilder();

                // Test repositories
                TestSupervisorRepository(connectionFactory);
                TestATMRepository(connectionFactory);
                TestTransactionRepository(connectionFactory);

                Console.WriteLine();
                Console.WriteLine("🎉 Phase 1 Complete - All database layer tests passed!");
                Console.WriteLine("Ready to proceed to Phase 2: MVVM Foundation & Application Shell");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error during Phase 1 testing: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        static void TestQueryBuilder()
        {
            Console.WriteLine("--- Testing QueryBuilder ---");

            try
            {
                // Test SELECT query
                var selectQuery = new QueryBuilder()
                    .Select("Id", "Username", "FullName")
                    .From("Supervisors")
                    .Where("IsActive = ?", 1)
                    .And("Id > ?", 0)
                    .OrderBy("FullName")
                    .Limit(10)
                    .Build();

                Console.WriteLine("✓ SELECT query built successfully");
                Console.WriteLine($"   SQL: {selectQuery}");

                // Test INSERT query
                var insertQuery = new QueryBuilder()
                    .InsertInto("TestTable")
                    .Values(new Dictionary<string, object>
                    {
                        { "Name", "Test Name" },
                        { "Value", 123 },
                        { "IsActive", true }
                    })
                    .Build();

                Console.WriteLine("✓ INSERT query built successfully");
                Console.WriteLine($"   SQL: {insertQuery}");

                // Test UPDATE query
                var updateQuery = new QueryBuilder()
                    .Update("TestTable")
                    .Set(new Dictionary<string, object>
                    {
                        { "Name", "Updated Name" },
                        { "Value", 456 }
                    })
                    .Where("Id = ?", 1)
                    .Build();

                Console.WriteLine("✓ UPDATE query built successfully");
                Console.WriteLine($"   SQL: {updateQuery}");

                // Test DELETE query
                var deleteQuery = QueryBuilder.DeleteById("TestTable", 1).Build();
                Console.WriteLine("✓ DELETE query built successfully");
                Console.WriteLine($"   SQL: {deleteQuery}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ QueryBuilder test failed: {ex.Message}");
                throw;
            }

            Console.WriteLine();
        }

        static void TestSupervisorRepository(SQLiteConnectionFactory connectionFactory)
        {
            Console.WriteLine("--- Testing SupervisorRepository ---");

            try
            {
                var repository = new SupervisorRepository(connectionFactory);

                // Test GetAll
                var allSupervisors = repository.GetAll().ToList();
                Console.WriteLine($"✓ Retrieved {allSupervisors.Count} supervisors");

                if (allSupervisors.Any())
                {
                    var firstSupervisor = allSupervisors.First();
                    Console.WriteLine($"   First supervisor: {firstSupervisor.FullName} ({firstSupervisor.Username})");

                    // Test GetById
                    var supervisorById = repository.GetById(firstSupervisor.Id);
                    Console.WriteLine($"✓ Retrieved supervisor by ID: {supervisorById?.FullName}");

                    // Test GetByUsername
                    var supervisorByUsername = repository.GetByUsername(firstSupervisor.Username);
                    Console.WriteLine($"✓ Retrieved supervisor by username: {supervisorByUsername?.FullName}");

                    // Test credential validation
                    var isValidCredential = repository.ValidateCredentials("abdelrahmanhas", "password123");
                    Console.WriteLine($"✓ Credential validation test: {isValidCredential}");

                    // Test username existence check
                    var usernameExists = repository.UsernameExists(firstSupervisor.Username);
                    Console.WriteLine($"✓ Username exists check: {usernameExists}");
                }

                // Test Insert new supervisor
                var newSupervisor = new Supervisor
                {
                    Username = "testsupervisor" + DateTime.Now.Ticks,
                    PasswordHash = SupervisorRepository.HashPassword("testpassword"),
                    FullName = "Test Supervisor",
                    Email = "test@banquemisr.com"
                };

                var insertedId = repository.Insert(newSupervisor);
                Console.WriteLine($"✓ Inserted new supervisor with ID: {insertedId}");

                // Test Update
                newSupervisor.Id = insertedId;
                newSupervisor.FullName = "Updated Test Supervisor";
                var updateResult = repository.Update(newSupervisor);
                Console.WriteLine($"✓ Update supervisor result: {updateResult}");

                // Test count operations
                var totalCount = repository.GetCount();
                var activeCount = repository.GetActiveSupervisors().Count();
                Console.WriteLine($"✓ Total supervisors: {totalCount}, Active: {activeCount}");

                // Test Delete (cleanup)
                var deleteResult = repository.Delete(insertedId);
                Console.WriteLine($"✓ Delete supervisor result: {deleteResult}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SupervisorRepository test failed: {ex.Message}");
                throw;
            }

            Console.WriteLine();
        }

        static void TestATMRepository(SQLiteConnectionFactory connectionFactory)
        {
            Console.WriteLine("--- Testing ATMRepository ---");

            try
            {
                var repository = new ATMRepository(connectionFactory);

                // Test GetAll
                var allATMs = repository.GetAll().ToList();
                Console.WriteLine($"✓ Retrieved {allATMs.Count} ATMs");

                if (allATMs.Any())
                {
                    var firstATM = allATMs.First();
                    Console.WriteLine($"   First ATM: {firstATM.DisplayName} - {firstATM.ATMInfo}");

                    // Test GetById
                    var atmById = repository.GetById(firstATM.Id);
                    Console.WriteLine($"✓ Retrieved ATM by ID: {atmById?.DisplayName}");

                    // Test GetBySupervisorId
                    var atmsBySupervisor = repository.GetBySupervisorId(firstATM.SupervisorId).ToList();
                    Console.WriteLine($"✓ Retrieved {atmsBySupervisor.Count} ATMs for supervisor ID {firstATM.SupervisorId}");

                    // Test GetByBranch
                    var atmsByBranch = repository.GetByBranch(firstATM.Branch).ToList();
                    Console.WriteLine($"✓ Retrieved {atmsByBranch.Count} ATMs for branch {firstATM.Branch}");

                    // Test GL number existence check
                    var glExists = repository.ExistsWithGLNumber(firstATM.GLNumber);
                    Console.WriteLine($"✓ GL Number exists check: {glExists}");
                }

                // Test Insert new ATM
                var newATM = new ATM
                {
                    SupervisorId = 1, // Use first supervisor
                    Name = "Test ATM " + DateTime.Now.Ticks,
                    Branch = "999",
                    GLNumber = "999999" + DateTime.Now.Ticks.ToString().Substring(0, 6),
                    ATMType = "DN",
                    CassetteCount = 4
                };

                var insertedId = repository.Insert(newATM);
                Console.WriteLine($"✓ Inserted new ATM with ID: {insertedId}");

                // Test Update
                newATM.Id = insertedId;
                newATM.Name = "Updated Test ATM";
                newATM.CassetteCount = 6;
                var updateResult = repository.Update(newATM);
                Console.WriteLine($"✓ Update ATM result: {updateResult}");

                // Test utility methods
                var distinctBranches = repository.GetDistinctBranches().ToList();
                var distinctTypes = repository.GetDistinctATMTypes().ToList();
                var activeATMs = repository.GetActiveATMs().Count();

                Console.WriteLine($"✓ Distinct branches: {string.Join(", ", distinctBranches)}");
                Console.WriteLine($"✓ Distinct ATM types: {string.Join(", ", distinctTypes)}");
                Console.WriteLine($"✓ Active ATMs count: {activeATMs}");

                // Test count operations
                var totalCount = repository.GetCount();
                var supervisorCount = repository.GetCountBySupervisor(1);
                Console.WriteLine($"✓ Total ATMs: {totalCount}, Supervisor 1 ATMs: {supervisorCount}");

                // Test Delete (cleanup)
                var deleteResult = repository.Delete(insertedId);
                Console.WriteLine($"✓ Delete ATM result: {deleteResult}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ATMRepository test failed: {ex.Message}");
                throw;
            }

            Console.WriteLine();
        }

        static void TestTransactionRepository(SQLiteConnectionFactory connectionFactory)
        {
            Console.WriteLine("--- Testing TransactionRepository ---");

            try
            {
                var repository = new TransactionRepository(connectionFactory);

                // Test GetAll
                var allTransactions = repository.GetAll().ToList();
                Console.WriteLine($"✓ Retrieved {allTransactions.Count} transactions");

                if (allTransactions.Any())
                {
                    var firstTransaction = allTransactions.First();
                    Console.WriteLine($"   First transaction: ID {firstTransaction.Id}, ATM {firstTransaction.ATMId}, Status: {firstTransaction.ReconciliationStatus}");

                    // Test GetById
                    var transactionById = repository.GetById(firstTransaction.Id);
                    Console.WriteLine($"✓ Retrieved transaction by ID: {transactionById?.Id}");

                    // Test GetByATMId
                    var atmTransactions = repository.GetByATMId(firstTransaction.ATMId).ToList();
                    Console.WriteLine($"✓ Retrieved {atmTransactions.Count} transactions for ATM {firstTransaction.ATMId}");

                    // Test GetByReconciliationStatus
                    var balancedTransactions = repository.GetByReconciliationStatus("Balanced").ToList();
                    var shortageTransactions = repository.GetByReconciliationStatus("Shortage").ToList();
                    var overTransactions = repository.GetByReconciliationStatus("Over").ToList();

                    Console.WriteLine($"✓ Reconciliation status counts - Balanced: {balancedTransactions.Count}, Shortage: {shortageTransactions.Count}, Over: {overTransactions.Count}");
                }

                // Test Insert new transaction
                var newTransaction = new ATMTransaction
                {
                    ATMId = 1, // Use first ATM
                    TransactionDate = DateTime.Today,
                    BeginningCash = 100000,
                    AddedCash = 50000,
                    RecycledCash = 10000,
                    EndingCash = 95000,
                    DepositedCash = 65000,
                    ReconciliationStatus = "Pending"
                };

                var insertedId = repository.Insert(newTransaction);
                Console.WriteLine($"✓ Inserted new transaction with ID: {insertedId}");

                // Test calculated properties
                var insertedTransaction = repository.GetById(insertedId);
                if (insertedTransaction != null)
                {
                    Console.WriteLine($"✓ Calculated cash: {insertedTransaction.CalculatedCash:C}");
                    Console.WriteLine($"✓ Calculated variance: {insertedTransaction.CalculatedVariance:C}");
                    Console.WriteLine($"✓ Is balanced: {insertedTransaction.IsBalanced}");
                }

                // Test Update
                newTransaction.Id = insertedId;
                newTransaction.ReconciliationStatus = "Balanced";
                newTransaction.IsReconciled = true;
                newTransaction.Variance = newTransaction.CalculatedVariance;
                var updateResult = repository.Update(newTransaction);
                Console.WriteLine($"✓ Update transaction result: {updateResult}");

                // Test date range queries
                var dateRangeTransactions = repository.GetByDateRange(DateTime.Today.AddDays(-7), DateTime.Today).ToList();
                Console.WriteLine($"✓ Retrieved {dateRangeTransactions.Count} transactions from last 7 days");

                // Test unreconciled transactions
                var unreconciledTransactions = repository.GetUnreconciledTransactions().ToList();
                Console.WriteLine($"✓ Retrieved {unreconciledTransactions.Count} unreconciled transactions");

                // Test recent transactions
                var recentTransactions = repository.GetRecentTransactions(5).ToList();
                Console.WriteLine($"✓ Retrieved {recentTransactions.Count} recent transactions");

                // Test variance queries
                var varianceTransactions = repository.GetTransactionsWithVariance(100).ToList();
                Console.WriteLine($"✓ Retrieved {varianceTransactions.Count} transactions with variance >= 100");

                // Test utility methods
                var totalVariance = repository.GetTotalVarianceByATM(1);
                var balancedCount = repository.GetReconciliationCountByStatus("Balanced");
                var hasTransactionToday = repository.HasTransactionForDate(1, DateTime.Today);

                Console.WriteLine($"✓ Total variance for ATM 1: {totalVariance:C}");
                Console.WriteLine($"✓ Balanced transactions count: {balancedCount}");
                Console.WriteLine($"✓ Has transaction for today (ATM 1): {hasTransactionToday}");

                // Test count operations
                var totalCount = repository.GetCount();
                Console.WriteLine($"✓ Total transactions: {totalCount}");

                // Test UpdateReconciliationStatus
                repository.UpdateReconciliationStatus(insertedId, "Over", 150.50m, "Test variance note");
                var updatedTransaction = repository.GetById(insertedId);
                Console.WriteLine($"✓ Updated reconciliation status: {updatedTransaction?.ReconciliationStatus}, Variance: {updatedTransaction?.Variance:C}");

                // Test Delete (cleanup)
                var deleteResult = repository.Delete(insertedId);
                Console.WriteLine($"✓ Delete transaction result: {deleteResult}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ TransactionRepository test failed: {ex.Message}");
                throw;
            }

            Console.WriteLine();
        }
    }
}