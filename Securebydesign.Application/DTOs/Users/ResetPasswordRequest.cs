using System;
using System.Collections.Generic;
using System.Text;

namespace Securebydesign.Application.DTOs.Users
{
    public class ResetPasswordRequest
    {
        public string Email { get; set; }
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmNewPassword { get; set; }
    }
}
