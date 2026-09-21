using System;
using System.Collections.Generic;
using System.Text;

namespace Securebydesign.Application.DTOs.Users
{
    public class SignupResponse
    {
        public Guid Id { get; set; }
        public string Role { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
