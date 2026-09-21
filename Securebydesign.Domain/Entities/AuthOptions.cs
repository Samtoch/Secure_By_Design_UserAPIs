using System;
using System.Collections.Generic;
using System.Text;

namespace Securebydesign.Domain.Entities
{
    public class AuthOptions
    {
        public string? KeyOrName { get; set; }   // For Basic Auth
        public string? Value { get; set; }     // Used as password (Basic) or token (Bearer)
        public AuthType AuthType { get; set; }
    }

    public enum AuthType
    {
        None,
        Basic,
        Bearer,
        ApiKey
    }
}
