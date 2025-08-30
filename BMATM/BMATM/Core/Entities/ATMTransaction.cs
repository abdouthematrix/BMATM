using System;

namespace BMATM.Core.Entities
{
    public class ATMTransaction
    {
        public int Id { get; set; }
        public int ATMId { get; set; }
        public DateTime TransactionDate { get; set; }
        public decimal? BeginningCash { get; set; }
        public decimal? AddedCash { get; set; }
        public decimal? RecycledCash { get; set; }
        public decimal? EndingCash { get; set; }
        public decimal? DepositedCash { get; set; }
        public decimal? GLBalance { get; set; }
        public bool IsReconciled { get; set; }
        public string ReconciliationStatus { get; set; }
        public decimal? Variance { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedDate { get; set; }

        // Navigation property (not mapped to database)
        public ATM ATM { get; set; }

        public ATMTransaction()
        {
            TransactionDate = DateTime.Today;
            CreatedDate = DateTime.Now;
            IsReconciled = false;
            ReconciliationStatus = "Pending";
        }

        public decimal CalculatedCash
        {
            get
            {
                var beginning = BeginningCash ?? 0;
                var added = AddedCash ?? 0;
                var recycled = RecycledCash ?? 0;
                var deposited = DepositedCash ?? 0;

                return beginning + added + recycled - deposited;
            }
        }

        public decimal CalculatedVariance
        {
            get
            {
                if (!EndingCash.HasValue) return 0;
                return EndingCash.Value - CalculatedCash;
            }
        }

        public bool IsBalanced => Math.Abs(CalculatedVariance) <= 1.00m; // 1 LE tolerance

        public override string ToString()
        {
            return $"Transaction {Id} - {TransactionDate:dd/MM/yyyy} - {ReconciliationStatus}";
        }
    }
}