using System;
using System.Collections.Generic;
using System.Text;

namespace Securebydesign.Domain.Entities
{
    public class SecureToken
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }
        public string Purpose { get; set; } // "Signup" or "PasswordReset"
        public DateTime ExpiryDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
