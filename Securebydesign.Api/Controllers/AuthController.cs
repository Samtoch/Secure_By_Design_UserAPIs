using Securebydesign.Application.DTOs.Auth;
using Securebydesign.Application.DTOs.Users;
using Securebydesign.Application.Interfaces;
using Securebydesign.Application.Interfaces.AuthServices;
using Securebydesign.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Securebydesign.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // LOGIN
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var response = await _authService.LoginAsync(request);
            return StatusCode(response.StatusCode, response);
        }

        // AUTHORIZE TOKEN
        [AllowAnonymous]
        [HttpPost("authorize")]
        public async Task<IActionResult> AuthorizeToken([FromBody] ValidateAccessToken request)
        {
            var response = await _authService.AuthorizeTokenAsync(request);
            return StatusCode(response.StatusCode, response);
        }

        // REFRESH TOKEN
        [AllowAnonymous]
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            var response = await _authService.RefreshTokenAsync(request);
            return StatusCode(response.StatusCode, response);
        }

        [AllowAnonymous]
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            var response = await _authService.ForgotPasswordAsync(request);
            return StatusCode(response.StatusCode, response);
        }

        [Authorize]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var response = await _authService.ResetPasswordAsync(request);
            return StatusCode(response.StatusCode, response);
        }

        // VALIDATE TOKEN
        [HttpPost("validate-token")]
        public async Task<IActionResult> ValidateSignupToken([FromBody] ValidateTokenRequest token)
        {
            var response = await _authService.ValidateSignupTokenAsync(token);
            return StatusCode(response.StatusCode, response);
        }

        // REGENERATE TOKEN
        [HttpPost("regenerate-token")]
        public async Task<IActionResult> RegenerateUserToken([FromBody] RequestNewToken request)
        {
            var response = await _authService.RegenerateUserTokenAsync(request);
            return StatusCode(response.StatusCode, response);
        }

        // LOCK USER ACCOUNT
        [Authorize(Roles = "admin")]
        [HttpPut("{id:guid}/lock")]
        public async Task<IActionResult> LockUserAccount(Guid id)
        {
            var response = await _authService.LockUserAccountAsync(id);
            return StatusCode(response.StatusCode, response);
        }
    }
}