using System;

namespace BMATM.Core.Entities
{
    public class Supervisor
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastLoginDate { get; set; }

        public Supervisor()
        {
            IsActive = true;
            CreatedDate = DateTime.Now;
        }

        public override string ToString()
        {
            return $"{FullName} ({Username})";
        }
    }
}