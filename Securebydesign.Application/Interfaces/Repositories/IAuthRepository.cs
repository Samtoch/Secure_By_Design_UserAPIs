using Securebydesign.Application.DTOs.Auth;
using Securebydesign.Domain.Entities;
using Securebydesign.Domain.Entities.Auth;
using System;
using System.Collections.Generic;
using System.Text;

namespace Securebydesign.Application.Interfaces.Repositories
{
    public interface IAuthRepository
    {
        Task<bool> SaveSecureTokenAsync(SecureToken token);
        Task<SecureToken?> GetValidTokenAsync(string email, string token, string purpose);
        Task<bool> UpdatePasswordAsync(string email, string passwordHash);
        Task<bool> InvalidateTokenAsync(string email, string purpose);
        Task<SecureToken?> GetValidSignupTokenAsync(ValidateTokenRequest token);
        Task<bool> MarkUserAsVerifiedAsync(string email);
        Task<bool> LockUserAccountAsync(Guid id);

        Task<bool> SaveRefreshTokenAsync(RefreshToken refreshToken);
        Task<RefreshToken?> GetRefreshTokenAsync(string token);
        Task<bool> RevokeRefreshTokenAsync(string token);
    }

}
