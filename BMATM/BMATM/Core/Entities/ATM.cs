using System;

namespace BMATM.Core.Entities
{
    public class ATM
    {
        public int Id { get; set; }
        public int SupervisorId { get; set; }
        public string Name { get; set; }
        public string Branch { get; set; }
        public string GLNumber { get; set; }
        public string ATMType { get; set; }
        public int CassetteCount { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }

        // Navigation property (not mapped to database)
        public Supervisor Supervisor { get; set; }

        public ATM()
        {
            IsActive = true;
            CreatedDate = DateTime.Now;
            CassetteCount = 4; // Default cassette count
        }

        public string DisplayName => $"{Name} - {Branch}";
        public string ATMInfo => $"GL: {GLNumber} | Type: {ATMType} | Cassettes: {CassetteCount}";

        public override string ToString()
        {
            return DisplayName;
        }
    }
}