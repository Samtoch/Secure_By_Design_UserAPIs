using AutoMapper;
using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Securebydesign.Application.DTOs.Generic;
using Securebydesign.Application.DTOs.Users;
using Securebydesign.Application.Helpers;
using Securebydesign.Application.Interfaces;
using Securebydesign.Application.Interfaces.AuthServices;
using Securebydesign.Application.Interfaces.EmailServices;
using Securebydesign.Application.Interfaces.Repositories;
using Securebydesign.Application.Services.AuthServices;
using Securebydesign.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Securebydesign.Application.Services
{
    public class UserService : IUserService
    {
        private readonly ILogger<UserService> _logger;
        private readonly IEmailService _emailService;
        private readonly IAuthRepository _authRepository;
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;

        public UserService(IPasswordHasher passwordHasher, IMapper mapper, ILogger<UserService> logger, 
            IAuthRepository authRepository, IEmailService emailService, IUserRepository userRepository, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _userRepository = userRepository;
            _authRepository = authRepository;
            _passwordHasher = passwordHasher;
            _emailService = emailService;
            _logger = logger;
            _mapper = mapper;
        }

        // GET ALL USERS
        public async Task<ApiResponse<IEnumerable<LoginResponse>>> GetAllUsersAsync()
        {
            try
            {
                var users = await _userRepository.GetAllUsersAsync();
                var userResponses = _mapper.Map<IEnumerable<LoginResponse>>(users);

                return new ApiResponse<IEnumerable<LoginResponse>>
                {
                    Message = "Users retrieved successfully",
                    StatusCode = 200,
                    Flag = true,
                    Data = userResponses
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all users");

                return new ApiResponse<IEnumerable<LoginResponse>>
                {
                    Message = "An error occurred",
                    StatusCode = 500,
                    Flag = false,
                    Data = null
                };
            }
        }

        // GET BY ID
        public async Task<ApiResponse<LoginResponse>> GetUserByIdAsync(Guid id)
        {
            try
            {
                var user = await _userRepository.GetUserByIdAsync(id);
                var userResponse = _mapper.Map<LoginResponse>(user);

                if (user == null)
                {
                    return new ApiResponse<LoginResponse>
                    {
                        Message = "User not found",
                        StatusCode = 404,
                        Flag = false,
                        Data = null
                    };
                }

                return new ApiResponse<LoginResponse>
                {
                    Message = "User retrieved successfully",
                    StatusCode = 200,
                    Flag = true,
                    Data = userResponse
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user {Id}", id);

                return new ApiResponse<LoginResponse>
                {
                    Message = "An error occurred",
                    StatusCode = 500,
                    Flag = false,
                    Data = null
                };
            }
        }

        // GET BY EMAIL
        public async Task<ApiResponse<LoginResponse>> GetUserByEmailAsync(string email)
        {
            try
            {
                var user = await _userRepository.GetUserByEmailAsync(email);
                var userResponse = _mapper.Map<LoginResponse>(user);

                if (user == null)
                {
                    return new ApiResponse<LoginResponse>
                    {
                        Message = "User not found",
                        StatusCode = 404,
                        Flag = false,
                        Data = null
                    };
                }

                return new ApiResponse<LoginResponse>
                {
                    Message = "User retrieved successfully",
                    StatusCode = 200,
                    Flag = true,
                    Data = userResponse
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by email {Email}", email);

                return new ApiResponse<LoginResponse>
                {
                    Message = "An error occurred",
                    StatusCode = 500,
                    Flag = false,
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<IEnumerable<LoginResponse>>> GetUserByEmail(string email)
        {
            try
            {
                var user = await _userRepository.GetUserByEmail(email);

                if (user == null)
                {
                    return new ApiResponse<IEnumerable<LoginResponse>>
                    {
                        Message = "User not found",
                        StatusCode = 404,
                        Flag = false,
                        Data = null
                    };
                }

                var userResponse = _mapper.Map<IEnumerable<LoginResponse>>(user);   // user is non-null here

                return new ApiResponse<IEnumerable<LoginResponse>>
                {
                    Message = "User retrieved successfully",
                    StatusCode = 200,
                    Flag = true,
                    Data = userResponse
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by email");   // no email in the log
                return new ApiResponse<IEnumerable<LoginResponse>>
                {
                    Message = "An error occurred",
                    StatusCode = 500,
                    Flag = false,
                    Data = null
                };
            }
        }

        // GET BY PHONE
        public async Task<ApiResponse<LoginResponse>> GetUserByPhoneAsync(string phone)
        {
            try
            {
                var user = await _userRepository.GetUserByPhoneAsync(phone);
                var userResponse = _mapper.Map<LoginResponse>(user);

                if (user == null)
                {
                    return new ApiResponse<LoginResponse>
                    {
                        Message = "User not found",
                        StatusCode = 404,
                        Flag = false,
                        Data = null
                    };
                }

                return new ApiResponse<LoginResponse>
                {
                    Message = "User retrieved successfully",
                    StatusCode = 200,
                    Flag = true,
                    Data = userResponse
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by phone {Phone}", phone);

                return new ApiResponse<LoginResponse>
                {
                    Message = "An error occurred",
                    StatusCode = 500,
                    Flag = false,
                    Data = null
                };
            }
        }

        // GET BY ROLE
        public async Task<ApiResponse<IEnumerable<LoginResponse>>> GetUsersByRoleAsync(string role)
        {
            try
            {
                var users = await _userRepository.GetUserByRoleAsync(role);
                var userResponses = _mapper.Map<IEnumerable<LoginResponse>>(users);

                return new ApiResponse<IEnumerable<LoginResponse>>
                {
                    Message = "Users retrieved successfully",
                    StatusCode = 200,
                    Flag = true,
                    Data = userResponses
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users by role {Role}", role);

                return new ApiResponse<IEnumerable<LoginResponse>>
                {
                    Message = "An error occurred",
                    StatusCode = 500,
                    Flag = false,
                    Data = null
                };
            }
        }

        // CREATE USER
        public async Task<ApiResponse<SignupResponse>> CreateUserAsync(SignupRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Email))
                {
                    return new ApiResponse<SignupResponse>
                    {
                        Message = "Invalid request",
                        StatusCode = 400,
                        Flag = false,
                        Data = null
                    };
                }

                var user = await _userRepository.GetUserByEmailAsync(request.Email);
                if(user != null)
                    return new ApiResponse<SignupResponse>
                    {
                        Message = "User already exist",
                        StatusCode = 400,
                        Flag = false,
                        Data = null
                    };

                // Hash password before saving
                request.Password = _passwordHasher.Hash(request.Password);



                var res = await _userRepository.CreateUserAsync(request);

                // Sedding email logic can be implemented here using an email service
                DateTime oneYear = DateTime.Now.AddMinutes(10);
                var token = new SecureToken() { Email = request.Email, Purpose = "Signup", ExpiryDate = oneYear, Token = Guid.NewGuid().ToString()};
                await _authRepository.SaveSecureTokenAsync(token);
                //await _emailService.SendSignupEmail(request.Email, request.FirstName, token.Token);   

                return new ApiResponse<SignupResponse>
                {
                    Message = "User created successfully",
                    StatusCode = 201,
                    Flag = true,
                    Data = res
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user {Email}", request?.Email);

                return new ApiResponse<SignupResponse>
                {
                    Message = "An error occurred",
                    StatusCode = 500,
                    Flag = false,
                    Data = null
                };
            }
        }

        // UPDATE USER
        public async Task<ApiResponse<bool>> UpdateUserAsync(UpdateRequest user, Guid id)
        {
            try
            {
                if (user == null || id == Guid.Empty)
                {
                    await AuditAsync("Invalid request", 400);
                    return new ApiResponse<bool> { Message = "Invalid request", StatusCode = 400, Flag = false, Data = false };
                }

                var rec = await _userRepository.GetUserByIdAsync(id);
                if (rec == null)   // FIX: was "!= null", which rejected every existing user
                {
                    await AuditAsync("UserUpdateFailed", 404);
                    return new ApiResponse<bool> { Message = "User does not exist", StatusCode = 404, Flag = false, Data = false };
                }

                var updated = await _userRepository.UpdateUserAsync(user, id);

                await AuditAsync(updated ? "User Updated" : "User Update Failed", updated ? 200 : 404);

                return new ApiResponse<bool>
                {
                    Message = updated ? "User updated successfully" : "User not found",
                    StatusCode = updated ? 200 : 404,
                    Flag = updated,
                    Data = updated
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {id}", id);
                return new ApiResponse<bool> { Message = "An error occurred", StatusCode = 500, Flag = false, Data = false };
            }
        }

        // DELETE USER (SOFT DELETE)
        public async Task<ApiResponse<bool>> DeleteUserAsync(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return new ApiResponse<bool>
                    {
                        Message = "Invalid Id",
                        StatusCode = 400,
                        Flag = false,
                        Data = false
                    };
                }

                var deleted = await _userRepository.DeleteUserAsync(id);

                return new ApiResponse<bool>
                {
                    Message = deleted ? "User deleted successfully" : "User not found",
                    StatusCode = deleted ? 200 : 404,
                    Flag = deleted,
                    Data = deleted
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {Id}", id);

                return new ApiResponse<bool>
                {
                    Message = "An error occurred",
                    StatusCode = 500,
                    Flag = false,
                    Data = false
                };
            }
        }

        private async Task AuditAsync(string eventName, int status, Guid? userId = null, string? role = null)
        {
            var http = _httpContextAccessor.HttpContext;

            await _userRepository.SaveAuditLogAsync(new AuditLog
            {
                Event = eventName,
                UserId = userId ?? (Guid.TryParse(http?.User.FindFirst("sub").Value, out var id) ? id : null),
                Role = role ?? http?.User.FindFirst("role").Value,
                Endpoint = http == null ? null : $"{http.Request.Method} {http.Request.Path}",
                Status = status,
                Ip = http?.Connection.RemoteIpAddress?.ToString(),
                CorrelationId = http?.TraceIdentifier
            });
        }
    }
}