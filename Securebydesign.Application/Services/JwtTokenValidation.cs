using Microsoft.IdentityModel.Tokens;
using Securebydesign.Application.DTOs.Auth;
using System;
using System.Collections.Generic;
using System.Text;

namespace Securebydesign.Application.Services
{

    /// <summary>
    /// One source of truth for the signing key and validation rules,
    /// used by both Program.cs (JwtBearer middleware) and JwtService.
    /// </summary>
    public static class JwtTokenValidation
    {
        /// <summary>
        /// The key must be a Base64-encoded random value of at least 32 bytes (256 bits).
        /// Generate one with:  openssl rand -base64 32
        /// Store it in user secrets / environment variables / a vault, not in appsettings.json.
        /// </summary>
        public static SymmetricSecurityKey CreateSigningKey(string base64Key)
        {
            byte[] keyBytes;
            try
            {
                keyBytes = Convert.FromBase64String(base64Key);
            }
            catch (FormatException)
            {
                throw new InvalidOperationException("JwtSettings:Key must be a Base64-encoded random key.");
            }

            if (keyBytes.Length < 32)
                throw new InvalidOperationException("JwtSettings:Key must be at least 256 bits (32 bytes).");

            return new SymmetricSecurityKey(keyBytes);
        }

        public static TokenValidationParameters CreateParameters(
            JwtSettings settings, SecurityKey signingKey, bool validateLifetime = true) => new()
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,
                ValidAlgorithms = [SecurityAlgorithms.HmacSha256],   // reject "none" and algorithm swaps
                RequireSignedTokens = true,

                ValidateIssuer = true,
                ValidIssuer = settings.Issuer,
                ValidateAudience = true,
                ValidAudience = settings.Audience,

                ValidateLifetime = validateLifetime,
                RequireExpirationTime = true,
                ClockSkew = TimeSpan.Zero,

                // Must match the claim names JwtService writes
                RoleClaimType = AppClaimTypes.Role,
                NameClaimType = AppClaimTypes.Name
            };
    }
}