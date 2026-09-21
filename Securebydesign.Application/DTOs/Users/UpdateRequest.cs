using System;
using System.Collections.Generic;
using System.Text;

namespace Securebydesign.Application.DTOs.Users
{
    public class UpdateRequest
    {
        public string Username { get; set; }
        public string Role { get; set; }
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool LockoutEnabled { get; set; }
        public int AccessFailedCount { get; set; }
        public DateTime? LockoutEnd { get; set; }
        public string? NormalizedEmail { get; set; }
        public string? NormalizedUsername { get; set; }
        public string? PhoneNumber { get; set; }
        public bool TwoFactorEnabled { get; set; }

        public int NeighborhoodId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string City { get; set; }
    }
}
