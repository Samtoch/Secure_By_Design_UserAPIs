using System;
using System.Collections.Generic;
using System.Text;

namespace Securebydesign.Application.DTOs.Auth
{
    public class RequestNewToken
    {
        public string Email { get; set; }
        public string Purpose { get; set; }
    }
}
