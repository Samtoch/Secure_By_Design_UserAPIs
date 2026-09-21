using System;
using System.Collections.Generic;
using System.Text;

namespace Securebydesign.Application.DTOs.Auth
{
    public class ValidateTokenRequest
    {
        public string Token { get; set; }
        public string Purpose { get; set; }

    }
}
