namespace Securebydesign.Application.DTOs.Users
{
    public class LoginResponse
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }
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
        public string? PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public string? SecurityStamp { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }

        public string? Token { get; set; }
        public DateTime? ExpiresAt { get; set; }

        public string? RefreshToken { get; set; } 
        public int NeighborhoodId { get; set; } // Include NeighborhoodId in the response
        public string City { get; set; }
    }
}
