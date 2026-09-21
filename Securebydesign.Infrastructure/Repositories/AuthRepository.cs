using Securebydesign.Application.DTOs;
using Securebydesign.Application.DTOs.Auth;
using Securebydesign.Application.DTOs.Users;
using Securebydesign.Application.Interfaces;
using Securebydesign.Application.Interfaces.Repositories;
using Securebydesign.Domain.Entities;
using Securebydesign.Domain.Entities.Auth;
using Azure;
using Dapper;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Text;

namespace Securebydesign.Infrastructure.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly ILogger<AuthRepository> _logger;
        private readonly string _connectionString;

        public AuthRepository(ILogger<AuthRepository> logger, IConfiguration config)
        {
            _logger = logger;
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        private NpgsqlConnection CreateConnection() => new(_connectionString);

        // SAVE SECURE TOKEN
        public async Task<bool> SaveSecureTokenAsync(SecureToken token)
        {
            // Invalidate any existing token for same email + purpose before inserting
            var sql = @"
                DELETE FROM securitytokens
                WHERE email = @Email AND purpose = @Purpose;

                INSERT INTO securitytokens (email, token, purpose, expirydate, createdat)
                VALUES (@Email, @Token, @Purpose, @ExpiryDate, NOW());
            ";
            try
            {
                using var conn = CreateConnection();
                var rows = await conn.ExecuteAsync(sql, new
                {
                    token.Email,
                    token.Token,
                    token.Purpose,
                    token.ExpiryDate
                });
                return rows > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving secure token for {Email}", token.Email);
                throw;
            }
        }

        // GET VALID TOKEN
        public async Task<SecureToken?> GetValidTokenAsync(string email, string token, string purpose)
        {
            var sql = @"
                SELECT *
                FROM securitytokens
                WHERE email    = @Email
                  AND token    = @Token
                  AND purpose  = @Purpose
                  AND expirydate > NOW()
                LIMIT 1;
            ";
            try
            {
                using var conn = CreateConnection();
                return await conn.QueryFirstOrDefaultAsync<SecureToken>(sql, new
                {
                    Email = email,
                    Token = token,
                    Purpose = purpose
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching token for {Email}", email);
                throw;
            }
        }

        // UPDATE PASSWORD
        public async Task<bool> UpdatePasswordAsync(string email, string passwordHash)
        {
            var sql = @"
                UPDATE users
                SET passwordhash  = @PasswordHash,
                    password      = @PasswordHash,
                    datemodified  = NOW()
                WHERE email = @Email AND isdeleted = 0;
            ";
            try
            {
                using var conn = CreateConnection();
                var rows = await conn.ExecuteAsync(sql, new { Email = email, PasswordHash = passwordHash });
                return rows > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating password for {Email}", email);
                throw;
            }
        }

        // INVALIDATE TOKEN — moves it to history then deletes it
        public async Task<bool> InvalidateTokenAsync(string email, string purpose)
        {
            var sql = @"
                INSERT INTO securitytokens_history (originalid, email, token, purpose, expirydate, deletedat)
                SELECT id, email, token, purpose, expirydate, NOW()
                FROM securitytokens
                WHERE email = @Email AND purpose = @Purpose;

                DELETE FROM securitytokens
                WHERE email = @Email AND purpose = @Purpose;
            ";
            try
            {
                using var conn = CreateConnection();
                var rows = await conn.ExecuteAsync(sql, new { Email = email, Purpose = purpose });
                return rows > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error invalidating token for {Email}", email);
                throw;
            }
        }

        // GET VALID SIGNUP TOKEN
        public async Task<SecureToken?> GetValidSignupTokenAsync(ValidateTokenRequest token)
        {
            var sql = @"
                SELECT *
                FROM securitytokens
                WHERE token   = @Token
                  AND purpose = @Purpose
                  AND expirydate > NOW()
                LIMIT 1;
            ";
            try
            {
                using var conn = CreateConnection();
                var result = await conn.QueryFirstOrDefaultAsync<SecureToken>(sql, new { token.Token, token.Purpose });
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching signup token");
                throw;
            }
        }

        // MARK USER AS VERIFIED
        public async Task<bool> MarkUserAsVerifiedAsync(string email)
        {
            var sql = @"
                UPDATE users
                SET emailconfirmed = true,
                    datemodified   = NOW()
                WHERE email = @Email AND isdeleted = 0;
            ";
            try
            {
                using var conn = CreateConnection();
                var rows = await conn.ExecuteAsync(sql, new { Email = email });
                return rows > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking user as verified for {Email}", email);
                throw;
            }
        }

        // LOCK USER ACCOUNT
        public async Task<bool> LockUserAccountAsync(Guid id)
        {
            var sql = @"
                UPDATE users
                SET islocked = true,
                    datemodified   = NOW()
                WHERE id = @Id AND isdeleted = 0;
            ";
            try
            {
                using var conn = CreateConnection();
                var rows = await conn.ExecuteAsync(sql, new { Id = id });
                return rows > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error locking user for {Id}", id);
                throw;
            }
        }

        // SAVE REFRESH TOKEN
        public async Task<bool> SaveRefreshTokenAsync(RefreshToken refreshToken)
        {
            var sql = @"
                INSERT INTO refreshtokens (user_id, token, expiry_date, is_revoked, created_at)
                VALUES (@UserId, @Token, @ExpiryDate, false, NOW());
            ";
            try
            {
                using var conn = CreateConnection();
                var rows = await conn.ExecuteAsync(sql, new
                {
                    refreshToken.UserId,
                    refreshToken.Token,
                    refreshToken.ExpiryDate
                });
                return rows > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving refresh token for user {UserId}", refreshToken.UserId);
                throw;
            }
        }

        // GET REFRESH TOKEN
        public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
        {
            var sql = @"
                SELECT user_id AS UserId, token, expiry_date AS ExpiryDate, is_revoked AS IsRevoked, created_at AS CreatedAt
                FROM refreshtokens
                WHERE token      = @Token
                  AND is_revoked = false
                  AND expiry_date > NOW()
                LIMIT 1;
            ";
            try
            {
                using var conn = CreateConnection();
                return await conn.QueryFirstOrDefaultAsync<RefreshToken>(sql, new { Token = token });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching refresh token");
                throw;
            }
        }

        // REVOKE REFRESH TOKEN
        public async Task<bool> RevokeRefreshTokenAsync(string token)
        {
            var sql = @"
                UPDATE refreshtokens
                SET is_revoked = true
                WHERE token = @Token;
            ";
            try
            {
                using var conn = CreateConnection();
                var rows = await conn.ExecuteAsync(sql, new { Token = token });
                return rows > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking refresh token");
                throw;
            }
        }
    }
}