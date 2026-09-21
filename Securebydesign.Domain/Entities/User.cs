namespace Securebydesign.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string? Password { get; set; }
        public string? Role { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public int AccessFailedCount { get; set; }
        public string? ConcurrencyStamp { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool IsLocked { get; set; } // Custom field to track if the account is locked by user or admin
        public bool LockoutEnabled { get; set; }
        public DateTime? LockoutEnd { get; set; }
        public string? NormalizedEmail { get; set; }
        public string? NormalizedUsername { get; set; }
        public string? PasswordHash { get; set; }
        public string? PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public string? SecurityStamp { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public int IsDeleted { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int NeighborhoodId { get; set; }
        public string City { get; set; }

    }
}
