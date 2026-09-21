using System;
using System.Collections.Generic;
using System.Text;

namespace Securebydesign.Application.DTOs.Auth
{
    public class TokenResponse
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
    }
}
