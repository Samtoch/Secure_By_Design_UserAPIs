using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
//using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Securebydesign.Application.DTOs.Auth;
using Securebydesign.Application.Interfaces;
using Securebydesign.Domain.Entities;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Securebydesign.Application.Services
{
    /// <summary>
    /// Issues and reads tokens. It decides WHAT goes into a token;
    /// what a token is ALLOWED to do is decided by the policies in the API project.
    /// </summary>
    public class JwtService : IJwtService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<JwtService> _logger;
        private readonly SymmetricSecurityKey _signingKey;

        public JwtService(IOptions<JwtSettings> jwtSettings, ILogger<JwtService> logger)
        {
            _jwtSettings = jwtSettings.Value;
            _logger = logger;
            _signingKey = JwtTokenValidation.CreateSigningKey(_jwtSettings.Key);
        }

        // GENERATE TOKEN
        public TokenResponse GenerateToken(User user)
        {
            var now = DateTime.UtcNow;                       // UTC, not local time
            var expiry = now.AddMinutes(_jwtSettings.ExpiryMinutes);

            // Least privilege: a user with no role gets the lowest one, never an empty claim.
            string role = string.IsNullOrWhiteSpace(user.Role) ? Roles.User : user.Role;

            // Only what authorisation needs. The payload is readable by anyone,
            // so no email, names or other personal data.
            var claims = new[]
            {
                new Claim(AppClaimTypes.UserId,         user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti,  Guid.NewGuid().ToString()),
                new Claim(AppClaimTypes.Role,           role),
                new Claim(AppClaimTypes.Name,           user.Username ?? string.Empty),
            };

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                notBefore: now,
                expires: expiry,
                signingCredentials: new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256));

            return new TokenResponse
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
                RefreshToken = GenerateRefreshToken(),
                ExpiresAt = expiry,
                Email = user.Email,   // fine in the response body; just not inside the token
                Role = role
            };
        }

        // VALIDATE TOKEN
        // Note: the JwtBearer middleware already validates every request's token.
        // Keep this only if something outside the request pipeline needs it.
        public bool ValidateToken(string token)
        {
            try
            {
                CreateHandler().ValidateToken(
                    token, JwtTokenValidation.CreateParameters(_jwtSettings, _signingKey), out _);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Token validation failed");
                return false;
            }
        }

        // GET PRINCIPAL FROM (POSSIBLY EXPIRED) TOKEN, for the refresh flow only.
        // Signature, issuer, audience and algorithm are still checked; only expiry is skipped.
        // The caller MUST also verify the refresh token against the stored value for this user.
        public ClaimsPrincipal? GetPrincipalFromToken(string token)
        {
            try
            {
                return CreateHandler().ValidateToken(
                    token,
                    JwtTokenValidation.CreateParameters(_jwtSettings, _signingKey, validateLifetime: false),
                    out _);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to extract principal from token");
                return null;
            }
        }

        // GENERATE REFRESH TOKEN
        // Store only its SHA-256 hash in the database (e.g. Sha256Hasher.HashToHex),
        // rotate it on every use, and give it an expiry.
        public string GenerateRefreshToken() =>
            Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(64));   // URL-safe, no + or /

        // Keep short claim names ("sub", "role") exactly as written
        private static JwtSecurityTokenHandler CreateHandler() => new() { MapInboundClaims = false };
    }
}