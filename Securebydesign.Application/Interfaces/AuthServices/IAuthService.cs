using Securebydesign.Application.DTOs.Generic;
using Securebydesign.Application.DTOs.Auth;
using Securebydesign.Application.DTOs.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace Securebydesign.Application.Interfaces.AuthServices
{
    public interface IAuthService
    {
        Task<ApiResponse<ForgotPasswordResponse>> ForgotPasswordAsync(ForgotPasswordRequest request);
        Task<ApiResponse<bool>> ResetPasswordAsync(ResetPasswordRequest request);
        Task<ApiResponse<bool>> ValidateSignupTokenAsync(ValidateTokenRequest token);
        Task<ApiResponse<bool>> LockUserAccountAsync(Guid id);
        Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request);
        Task<ApiResponse<TokenResponse>> AuthorizeTokenAsync(ValidateAccessToken request);

        Task<ApiResponse<TokenResponse>> RefreshTokenAsync(RefreshTokenRequest request);
        Task<ApiResponse<string>> RegenerateUserTokenAsync(RequestNewToken request);

    }
}
