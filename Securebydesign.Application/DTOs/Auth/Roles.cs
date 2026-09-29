using System;
using System.Collections.Generic;
using System.Text;

namespace Securebydesign.Application.DTOs.Auth
{
    /// <summary>
    /// Role names, shared by the layer that ISSUES tokens (JwtService)
    /// and the layer that CHECKS them (API authorisation policies).
    /// </summary>
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string Support = "Support";
        public const string User = "User";
    }

    /// <summary>Short JWT claim names, used identically when issuing and validating.</summary>
    public static class AppClaimTypes
    {
        public const string UserId = "sub";
        public const string Role = "role";
        public const string Name = "name";
    }
}
