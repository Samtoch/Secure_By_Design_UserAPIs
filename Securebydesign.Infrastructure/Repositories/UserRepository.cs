using Azure;
using Dapper;
using Dapper;
using Securebydesign.Application.DTOs;
using Securebydesign.Application.DTOs.Users;
using Securebydesign.Application.Interfaces;
using Securebydesign.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Text;

namespace Securebydesign.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ILogger<UserRepository> _logger;
        private readonly string _connectionString;

        public UserRepository(ILogger<UserRepository> logger, IConfiguration config)
        {
            _logger = logger;
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        private NpgsqlConnection CreateConnection() => new(_connectionString);

        // GET ALL
        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            var sql = "SELECT * FROM users WHERE isdeleted = 0";

            try
            {
                using var conn = CreateConnection();
                return await conn.QueryAsync<User>(sql);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error get all users. Query: {sql}", sql);
                throw;
            }
        }

        // GET BY ID
        public async Task<User?> GetUserByIdAsync(Guid id)
        {
            var sql = "SELECT * FROM users WHERE id = @Id AND isdeleted = 0";

            try
            {
                using var conn = CreateConnection();
                return await conn.QueryFirstOrDefaultAsync<User>(sql, new { Id = id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error get user by Id {Id}", id);
                throw;
            }
        }

        // GET BY EMAIL 
        public async Task<User?> GetUserByEmailAsync(string email)
        {
            var sql = "SELECT * FROM users WHERE email = @Email AND isdeleted = 0";

            try
            {
                using var conn = CreateConnection();
                var response = await conn.QueryFirstOrDefaultAsync<User>(sql, new { Email = email });
                return response;    
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error get user by Email {Email}", email);
                throw;
            }
        }

        // GET BY PHONE 
        public async Task<User?> GetUserByPhoneAsync(string phone)
        {
            var sql = "SELECT * FROM users WHERE phonenumber = @Phone AND isdeleted = 0";

            try
            {
                using var conn = CreateConnection();
                return await conn.QueryFirstOrDefaultAsync<User>(sql, new { Phone = phone });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error get user by Phone {Phone}", phone);
                throw;
            }
        }

        // GET BY ROLE
        public async Task<IEnumerable<User>> GetUserByRoleAsync(string role)
        {
            var sql = "SELECT * FROM users WHERE role = @Role AND isdeleted = 0";

            try
            {
                using var conn = CreateConnection();
                return await conn.QueryAsync<User>(sql, new { Role = role });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error get users by Role {Role}", role);
                throw;
            }
        }

        // LOGIN
        public async Task<User?> UserLoginAsync(LoginRequest user)
        {
            var sql = @"
                SELECT * FROM users 
                WHERE username = @Username 
                  AND password = @Password 
                  AND isdeleted = 0;
            ";

            try
            {
                using var conn = CreateConnection();

                var response = await conn.QueryFirstOrDefaultAsync<User>(sql, new  
                    { Username = user.Username, Password = user.Password}
                );

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for Username {Username}, {Sql}", user.Username, sql);
                throw;
            }
        }

        // CREATE
        public async Task<SignupResponse> CreateUserAsync(SignupRequest user)
        {
            var sql = @"
                INSERT INTO users (
                    id, email, username, password, passwordhash, role,
                    firstname, lastname, phonenumber,
                    datecreated, isdeleted
                )
                VALUES (
                    @Id, @Email, @Email, @Password, @Password, 'customer',
                    @FirstName, @LastName, @PhoneNumber,
                    NOW(), 0
                )
                RETURNING id;
            ";

            try
            {
                using var conn = CreateConnection();

                var id = Guid.NewGuid();

                var res = await conn.ExecuteScalarAsync<Guid>(sql, new
                {
                    Id = id,
                    user.Email,
                    user.Password,
                    user.FirstName,
                    user.LastName,
                    user.PhoneNumber
                });

                return new SignupResponse
                {
                    Id = res,
                    Email = user.Email,
                    Username = user.Email,
                    Role = "customer",
                    DateCreated = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user {Email}", user.Email);
                throw;
            }
        }

        // UPDATE
        public async Task<bool> UpdateUserAsync(UpdateRequest user, Guid Id)
        {
            var sql = @"
                UPDATE users
                SET username       = @Username,
                    role           = @Role,
                    firstname      = @FirstName,
                    lastname       = @LastName,
                    phonenumber    = @PhoneNumber,
                    neighborhoodid = @NeighborhoodId,
                    city           = @City,
                    datemodified   = NOW()
                WHERE id = @Id AND isdeleted = 0;
            ";

            try
            {
                using var conn = CreateConnection();

                var rows = await conn.ExecuteAsync(sql, new
                {
                    user.Username,
                    user.Role,
                    user.FirstName,
                    user.LastName,
                    user.PhoneNumber,
                    user.NeighborhoodId,
                    user.City,
                    Id
                });

                return rows > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {Id}", Id);
                throw;
            }
        }

        // DELETE (SOFT)
        public async Task<bool> DeleteUserAsync(Guid id)
        {
            var sql = @"
                UPDATE users
                SET isdeleted = 1,
                    datemodified = NOW()
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
                _logger.LogError(ex, "Error deleting user {Id}", id);
                throw;
            }
        }
    }
}