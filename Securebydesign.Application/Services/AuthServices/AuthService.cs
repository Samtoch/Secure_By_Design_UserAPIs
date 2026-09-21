using Securebydesign.Application.DTOs.Generic;
using Securebydesign.Application.DTOs.Auth;
using Securebydesign.Application.DTOs.Users;
using Securebydesign.Application.Helpers;
using Securebydesign.Application.Interfaces;
using Securebydesign.Application.Interfaces.AuthServices;
using Securebydesign.Application.Interfaces.EmailServices;
using Securebydesign.Application.Interfaces.Repositories;
using Securebydesign.Domain.Entities;
using Securebydesign.Domain.Entities.Auth;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Securebydesign.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IMapper _mapper;
        private readonly TokenSettings _tokenSettings;
        private readonly IJwtService _jwtService;
        private readonly ILogger<AuthService> _logger;
        private readonly IEmailService _emailService;
        private readonly IAuthRepository _authRepository;
        private readonly IUserRepository _userRepository;

        public AuthService(IMapper mapper, IJwtService jwtService, IOptions<TokenSettings> tokenSettings, 
            ILogger<AuthService> logger, IEmailService emailService, IAuthRepository authRepository, 
            IUserRepository userRepository)
        {
            _tokenSettings = tokenSettings.Value;
            _authRepository = authRepository;
            _userRepository = userRepository;
            _emailService = emailService;
            _jwtService = jwtService; 
            _logger = logger;
            _mapper = mapper;

        }

        // LOGIN
        public async Task<ApiResponse<LoginResponse>> LoginAsync_(LoginRequest request)
        {
            try
            {
                if (request == null
                    || string.IsNullOrWhiteSpace(request.Username)
                    || string.IsNullOrWhiteSpace(request.Password))
                    return new ApiResponse<LoginResponse>
                    {
                        Message = "Invalid login request",
                        StatusCode = 400,
                        Flag = false,
                        Data = null
                    };

                request.Password = Helper.ComputeStringToSha256Hash(request.Password);

                var user = await _userRepository.UserLoginAsync(request);
                if (user == null)
                    return new ApiResponse<LoginResponse>
                    {
                        Message = "Invalid username or password",
                        StatusCode = 401,
                        Flag = false,
                        Data = null
                    };

                var userResponse = _mapper.Map<LoginResponse>(user);
                var tokenResponse = _jwtService.GenerateToken(user);
                userResponse.Token = tokenResponse.AccessToken;
                userResponse.ExpiresAt = tokenResponse.ExpiresAt;

                return new ApiResponse<LoginResponse>
                {
                    Message = "Login successful",
                    StatusCode = 200,
                    Flag = true,
                    Data = userResponse
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for {Username}", request?.Username);
                return new ApiResponse<LoginResponse>
                {
                    Message = "An error occurred",
                    StatusCode = 500,
                    Flag = false,
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request)
        {
            try
            {
                if (request == null
                    || string.IsNullOrWhiteSpace(request.Username)
                    || string.IsNullOrWhiteSpace(request.Password))
                    return new ApiResponse<LoginResponse>
                    {
                        Message = "Invalid login request",
                        StatusCode = 400,
                        Flag = false,
                        Data = null
                    };

                request.Password = Helper.ComputeStringToSha256Hash(request.Password);

                var user = await _userRepository.UserLoginAsync(request);
                if (user == null)
                    return new ApiResponse<LoginResponse>
                    {
                        Message = "Invalid username or password",
                        StatusCode = 401,
                        Flag = false,
                        Data = null
                    };

                if (!user.EmailConfirmed)
                    return new ApiResponse<LoginResponse>
                    {
                        Message = "User not authorised. Please verify your account with your email",
                        StatusCode = 401,
                        Flag = false,
                        Data = null
                    };

                var tokenResponse = _jwtService.GenerateToken(user);

                // Persist the refresh token
                await _authRepository.SaveRefreshTokenAsync(new RefreshToken
                {
                    UserId = user.Id,
                    Token = tokenResponse.RefreshToken,
                    ExpiryDate = DateTime.UtcNow.AddDays(7)
                });

                var userResponse = _mapper.Map<LoginResponse>(user);
                userResponse.Token = tokenResponse.AccessToken;
                userResponse.RefreshToken = tokenResponse.RefreshToken;
                userResponse.ExpiresAt = tokenResponse.ExpiresAt;

                return new ApiResponse<LoginResponse>
                {
                    Message = "Login successful",
                    StatusCode = 200,
                    Flag = true,
                    Data = userResponse
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for {Username}", request?.Username);
                return new ApiResponse<LoginResponse>
                {
                    Message = "An error occurred",
                    StatusCode = 500,
                    Flag = false,
                    Data = null
                };
            }
        }

        // REFRESH TOKEN
        public async Task<ApiResponse<TokenResponse>> RefreshTokenAsync(RefreshTokenRequest request)
        {
            try
            {
                if (request == null
                    || string.IsNullOrWhiteSpace(request.AccessToken)
                    || string.IsNullOrWhiteSpace(request.RefreshToken))
                    return new ApiResponse<TokenResponse>
                    {
                        Message = "Invalid request",
                        StatusCode = 400,
                        Flag = false,
                        Data = null
                    };

                // Extract claims from the expired access token
                var principal = _jwtService.GetPrincipalFromToken(request.AccessToken);
                if (principal == null)
                    return new ApiResponse<TokenResponse>
                    {
                        Message = "Invalid access token",
                        StatusCode = 401,
                        Flag = false,
                        Data = null
                    };

                var userId = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
                if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var userGuid))
                    return new ApiResponse<TokenResponse>
                    {
                        Message = "Invalid token claims",
                        StatusCode = 401,
                        Flag = false,
                        Data = null
                    };

                // Validate the refresh token against the database
                var storedToken = await _authRepository.GetRefreshTokenAsync(request.RefreshToken);
                if (storedToken == null || storedToken.UserId != userGuid)
                    return new ApiResponse<TokenResponse>
                    {
                        Message = "Invalid or expired refresh token",
                        StatusCode = 401,
                        Flag = false,
                        Data = null
                    };

                var user = await _userRepository.GetUserByIdAsync(userGuid);
                if (user == null)
                    return new ApiResponse<TokenResponse>
                    {
                        Message = "User not found",
                        StatusCode = 404,
                        Flag = false,
                        Data = null
                    };

                // Revoke the old refresh token — one-time use
                await _authRepository.RevokeRefreshTokenAsync(request.RefreshToken);

                // Issue a brand new access + refresh token pair
                var newTokenResponse = _jwtService.GenerateToken(user);
                await _authRepository.SaveRefreshTokenAsync(new RefreshToken
                {
                    UserId = user.Id,
                    Token = newTokenResponse.RefreshToken,
                    ExpiryDate = DateTime.UtcNow.AddDays(7)
                });

                return new ApiResponse<TokenResponse>
                {
                    Message = "Token refreshed successfully",
                    StatusCode = 200,
                    Flag = true,
                    Data = newTokenResponse
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                return new ApiResponse<TokenResponse>
                {
                    Message = "An error occurred",
                    StatusCode = 500,
                    Flag = false,
                    Data = null
                };
            }
        }

        // AUTHORIZE TOKEN
        public async Task<ApiResponse<TokenResponse>> AuthorizeTokenAsync(ValidateAccessToken request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.AccessToken))
                    return new ApiResponse<TokenResponse>
                    {
                        Message = "Token is required",
                        StatusCode = 400,
                        Flag = false,
                        Data = null
                    };

                var isValid = _jwtService.ValidateToken(request.AccessToken);
                if (!isValid)
                    return new ApiResponse<TokenResponse>
                    {
                        Message = "Invalid or expired token",
                        StatusCode = 401,
                        Flag = false,
                        Data = null
                    };

                var principal = _jwtService.GetPrincipalFromToken(request.AccessToken);
                if (principal == null)
                    return new ApiResponse<TokenResponse>
                    {
                        Message = "Could not read token claims",
                        StatusCode = 401,
                        Flag = false,
                        Data = null
                    };

                var email = principal.FindFirst(ClaimTypes.Email)?.Value ?? principal.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
                var role = principal.FindFirst(ClaimTypes.Role)?.Value;
                var expClaim = principal.FindFirst(JwtRegisteredClaimNames.Exp)?.Value;


                DateTime? expiresAt = null;
                if (long.TryParse(expClaim, out var expUnix))
                    expiresAt = DateTimeOffset.FromUnixTimeSeconds(expUnix).UtcDateTime;

                return new ApiResponse<TokenResponse>
                {
                    Message = "Token is valid",
                    StatusCode = 200,
                    Flag = true,
                    Data = new TokenResponse
                    {
                        AccessToken = request.AccessToken,
                        Email = email ?? string.Empty,
                        Role = role ?? string.Empty,
                        ExpiresAt = expiresAt ?? DateTime.MinValue
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error authorizing token");
                return new ApiResponse<TokenResponse>
                {
                    Message = "An error occurred",
                    StatusCode = 500,
                    Flag = false,
                    Data = null
                };
            }
        }


        // FORGOT PASSWORD
        // Generates a reset token and returns it
        public async Task<ApiResponse<ForgotPasswordResponse>> ForgotPasswordAsync(ForgotPasswordRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Email))
                    return new ApiResponse<ForgotPasswordResponse>
                    {
                        Message = "Email is required",
                        StatusCode = 400,
                        Flag = false,
                        Data = null
                    };

                var user = await _userRepository.GetUserByEmailAsync(request.Email);

                // Return the same response whether the user exists or not — prevents email enumeration
                if (user == null)
                    return new ApiResponse<ForgotPasswordResponse>
                    {
                        Message = "If that email is registered, a reset token has been sent",
                        StatusCode = 200,
                        Flag = true,
                        Data = null
                    };

                int expTime = _tokenSettings.ExpiryMinutes;
                var token = new SecureToken
                {
                    Email = request.Email,
                    Token = Random.Shared.Next(100000, 999999).ToString(),
                    Purpose = "PasswordReset",
                    ExpiryDate = DateTime.UtcNow.AddMinutes(expTime)
                };

                await _authRepository.SaveSecureTokenAsync(token);

                // Send the reset email
                await _emailService.SendForgotPasswordEmail(request.Email, user.FirstName, token.Token);

                return new ApiResponse<ForgotPasswordResponse>
                {
                    Message = "A reset token has been sent",
                    StatusCode = 200,
                    Flag = true,
                    Data = new ForgotPasswordResponse
                    {
                        Email = request.Email,
                        Message = $"Reset token: {token.Token}"
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing forgot password for {Email}", request?.Email);
                return new ApiResponse<ForgotPasswordResponse>
                {
                    Message = "An error occurred",
                    StatusCode = 500,
                    Flag = false,
                    Data = null
                };
            }
        }

        // RESET PASSWORD
        public async Task<ApiResponse<bool>> ResetPasswordAsync(ResetPasswordRequest request)
        {
            try
            {
                if (request == null
                    || string.IsNullOrWhiteSpace(request.Email)
                    || string.IsNullOrWhiteSpace(request.CurrentPassword)
                    || string.IsNullOrWhiteSpace(request.NewPassword))
                    return new ApiResponse<bool>
                    {
                        Message = "Invalid request",
                        StatusCode = 400,
                        Flag = false,
                        Data = false
                    };

                if (request.NewPassword != request.ConfirmNewPassword)
                    return new ApiResponse<bool>
                    {
                        Message = "Passwords do not match",
                        StatusCode = 400,
                        Flag = false,
                        Data = false
                    };

                if (request.CurrentPassword == request.NewPassword)
                    return new ApiResponse<bool>
                    {
                        Message = "New password must be different from current password",
                        StatusCode = 400,
                        Flag = false,
                        Data = false
                    };

                var user = await _userRepository.GetUserByEmailAsync(request.Email);
                if (user == null)
                    return new ApiResponse<bool>
                    {
                        Message = "User not found",
                        StatusCode = 404,
                        Flag = false,
                        Data = false
                    };

                // Hash the incoming current password and compare against stored hash
                var currentHashed = Helper.ComputeStringToSha256Hash(request.CurrentPassword);
                if (user.PasswordHash != currentHashed)
                    return new ApiResponse<bool>
                    {
                        Message = "Current password is incorrect",
                        StatusCode = 400,
                        Flag = false,
                        Data = false
                    };

                var newHashed = Helper.ComputeStringToSha256Hash(request.NewPassword);
                var updated = await _authRepository.UpdatePasswordAsync(request.Email, newHashed);

                if (updated)
                    await _emailService.SendResetPasswordEmail(request.Email, user.FirstName);

                return new ApiResponse<bool>
                {
                    Message = updated ? "Password reset successfully" : "Failed to update password",
                    StatusCode = updated ? 200 : 500,
                    Flag = updated,
                    Data = updated
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password for {Email}", request?.Email);
                return new ApiResponse<bool>
                {
                    Message = "An error occurred",
                    StatusCode = 500,
                    Flag = false,
                    Data = false
                };
            }
        }

        // VALIDATE SIGNUP TOKEN
        public async Task<ApiResponse<bool>> ValidateSignupTokenAsync(ValidateTokenRequest token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token.Token))
                    return new ApiResponse<bool>
                    {
                        Message = "Token is required",
                        StatusCode = 400,
                        Flag = false,
                        Data = false
                    };

                var secureToken = await _authRepository.GetValidSignupTokenAsync(token);

                if (secureToken == null)
                    return new ApiResponse<bool>
                    {
                        Message = "Invalid or expired signup token",
                        StatusCode = 400,
                        Flag = false,
                        Data = false
                    };

                if (token.Purpose == "Signup")
                {
                    bool verified = await _authRepository.MarkUserAsVerifiedAsync(secureToken.Email);

                    if (!verified)
                        return new ApiResponse<bool>
                        {
                            Message = "Failed to verify user account",
                            StatusCode = 500,
                            Flag = false,
                            Data = false
                        };
                }
 
                // Archive and remove the used token
                await _authRepository.InvalidateTokenAsync(secureToken.Email, token.Purpose);

                return new ApiResponse<bool>
                {
                    Message = "Token validated successfully",
                    StatusCode = 200,
                    Flag = true,
                    Data = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating signup token");
                return new ApiResponse<bool>
                {
                    Message = "An error occurred",
                    StatusCode = 500,
                    Flag = false,
                    Data = false
                };
            }
        }

        // LOCK USER ACCOUNT
        public async Task<ApiResponse<bool>> LockUserAccountAsync(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return new ApiResponse<bool>
                    {
                        Message = "Invalid user ID",
                        StatusCode = 400,
                        Flag = false,
                        Data = false
                    };

                var user = await _userRepository.GetUserByIdAsync(id);
                if (user == null)
                    return new ApiResponse<bool>
                    {
                        Message = "User not found",
                        StatusCode = 404,
                        Flag = false,
                        Data = false
                    };

                if (user.IsLocked)
                    return new ApiResponse<bool>
                    {
                        Message = "User account is already locked",
                        StatusCode = 400,
                        Flag = false,
                        Data = false
                    };

                var locked = await _authRepository.LockUserAccountAsync(id);

                return new ApiResponse<bool>
                {
                    Message = locked ? "User account locked successfully" : "Failed to lock user account",
                    StatusCode = locked ? 200 : 500,
                    Flag = locked,
                    Data = locked
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error locking user account {Id}", id);
                return new ApiResponse<bool>
                {
                    Message = "An error occurred",
                    StatusCode = 500,
                    Flag = false,
                    Data = false
                };
            }
        }

        // REGENERATE TOKEN USER
        public async Task<ApiResponse<string>> RegenerateUserTokenAsync(RequestNewToken request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Purpose))
                {
                    return new ApiResponse<string>
                    {
                        Message = "Invalid request",
                        StatusCode = 400,
                        Flag = false,
                        Data = null
                    };
                }

                var user = await _userRepository.GetUserByEmailAsync(request.Email);
                if (user == null)
                    return new ApiResponse<string>
                    {
                        Message = "User does not exist",
                        StatusCode = 400,
                        Flag = false,
                        Data = null
                    };

                DateTime oneYear = DateTime.Now.AddMinutes(10);
                var token = new SecureToken() { Email = request.Email, Purpose = request.Purpose, ExpiryDate = oneYear, Token = Guid.NewGuid().ToString() };
                await _authRepository.SaveSecureTokenAsync(token);

                await _emailService.SendFreshTokenEmail(request.Email, user.FirstName, token.Token, token.Purpose);

                return new ApiResponse<string>
                {
                    Message = "New token created successfully",
                    StatusCode = 201,
                    Flag = true,
                    Data = null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating new token for {Email}", request?.Email);

                return new ApiResponse<string>
                {
                    Message = "An error occurred",
                    StatusCode = 500,
                    Flag = false,
                    Data = null
                };
            }
        }

    }
}
