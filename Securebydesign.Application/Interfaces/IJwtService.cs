using Securebydesign.Application.DTOs.Auth;
using Securebydesign.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace Securebydesign.Application.Interfaces
{
    public interface IJwtService
    {
        TokenResponse GenerateToken(User user);
        string GenerateRefreshToken();
        bool ValidateToken(string token);
        ClaimsPrincipal? GetPrincipalFromToken(string token);
    }
}
