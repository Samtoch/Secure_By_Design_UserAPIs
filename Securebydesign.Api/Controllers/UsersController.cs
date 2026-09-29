using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Securebydesign.Api.Extensions;
using Securebydesign.Application.DTOs.Auth;
using Securebydesign.Application.DTOs.Users;
using Securebydesign.Application.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Securebydesign.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Deny by default: every endpoint needs a valid token unless marked otherwise
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IAuthorizationService _authorizationService;

        public UsersController(IUserService userService, IAuthorizationService authorizationService)
        {
            _userService = userService;
            _authorizationService = authorizationService;
        }

        // GET ALL USERS: staff only
        [HttpGet]
        [Authorize(Policy = Policies.ViewAllUsers)]
        public async Task<IActionResult> GetAllUsers()
        {
            var response = await _userService.GetAllUsersAsync();
            return StatusCode(response.StatusCode, response);
        }

        // GET USER BY ID: own account, or Support/Admin
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            if (!await IsAllowedAsync(id, Policies.ViewUser))
                return Forbid();

            var response = await _userService.GetUserByIdAsync(id);
            return StatusCode(response.StatusCode, response);
        }

        // CREATE USER (SIGNUP): public, but rate-limited.
        [HttpPost]
        [AllowAnonymous]
        [EnableRateLimiting("signup")]
        public async Task<IActionResult> CreateUser([FromBody] SignupRequest request)
        {
            var response = await _userService.CreateUserAsync(request);
            return StatusCode(response.StatusCode, response);
        }

        // UPDATE USER: own account, or Admin. Profile fields only; no Role.
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateRequest user)
        {
            if (!await IsAllowedAsync(id, Policies.UpdateUser))
                return Forbid();

            var response = await _userService.UpdateUserAsync(user, id);
            return StatusCode(response.StatusCode, response);
        }

        // DELETE USER (SOFT DELETE): Admin only
        [HttpDelete("{id:guid}")]
        [Authorize(Policy = Policies.DeleteUsers)]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var response = await _userService.DeleteUserAsync(id);
            return StatusCode(response.StatusCode, response);
        }

        // GET USER BY EMAIL
        [HttpGet("email")]
        [Authorize]
        public async Task<IActionResult> GetUserByEmail([FromQuery] string email)
        {
            var response = await _userService.GetUserByEmail(email);
            return StatusCode(response.StatusCode, response);
        }

        // Runs a resource-based policy against the ID of the account being accessed
        private async Task<bool> IsAllowedAsync(Guid targetUserId, string policy)
        {
            var result = await _authorizationService.AuthorizeAsync(User, targetUserId, policy);
            return result.Succeeded;
        }
    }
}