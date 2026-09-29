using Securebydesign.Application.DTOs.Generic;
using Securebydesign.Application.DTOs.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace Securebydesign.Application.Interfaces
{
    public interface IUserService
    {
        Task<ApiResponse<IEnumerable<LoginResponse>>> GetAllUsersAsync();
        Task<ApiResponse<LoginResponse>> GetUserByIdAsync(Guid id);
        Task<ApiResponse<LoginResponse>> GetUserByEmailAsync(string email);
        Task<ApiResponse<IEnumerable<LoginResponse>>> GetUserByEmail(string email);
        Task<ApiResponse<LoginResponse>> GetUserByPhoneAsync(string phone);
        Task<ApiResponse<IEnumerable<LoginResponse>>> GetUsersByRoleAsync(string role);
        Task<ApiResponse<SignupResponse>> CreateUserAsync(SignupRequest request);
        Task<ApiResponse<bool>> UpdateUserAsync(UpdateRequest user, Guid id);
        Task<ApiResponse<bool>> DeleteUserAsync(Guid id);
    }
}
