using Securebydesign.Application.DTOs.Users;
using Securebydesign.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Securebydesign.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User?> GetUserByIdAsync(Guid id);
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> GetUserByPhoneAsync(string phone);
        Task<IEnumerable<User>> GetUserByRoleAsync(string role);
        Task<User?> UserLoginAsync(LoginRequest user);
        Task<SignupResponse> CreateUserAsync(SignupRequest user);
        Task<bool> UpdateUserAsync(UpdateRequest user, Guid Id);
        Task<bool> DeleteUserAsync(Guid id);
    }
}
