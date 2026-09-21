using Securebydesign.Application.DTOs.Users;
using Securebydesign.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Securebydesign.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        // GET ALL USERS
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllUsers()
        {
            var response = await _userService.GetAllUsersAsync();
            return StatusCode(response.StatusCode, response);
        }

        // GET USER BY ID
        [HttpGet("{id:guid}")]
        [Authorize]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            var response = await _userService.GetUserByIdAsync(id);
            return StatusCode(response.StatusCode, response);
        }

        // GET USER BY EMAIL
        [HttpGet("email")]
        [Authorize]
        public async Task<IActionResult> GetUserByEmail([FromQuery] string email)
        {
            var response = await _userService.GetUserByEmailAsync(email);
            return StatusCode(response.StatusCode, response);
        }

        // GET USER BY PHONE
        [HttpGet("phone")]
        [Authorize]
        public async Task<IActionResult> GetUserByPhone([FromQuery] string phone)
        {
            var response = await _userService.GetUserByPhoneAsync(phone);
            return StatusCode(response.StatusCode, response);
        }

        // GET USERS BY ROLE
        [Authorize]
        [HttpGet("role")]
        public async Task<IActionResult> GetUsersByRole([FromQuery] string role)
        {
            var response = await _userService.GetUsersByRoleAsync(role);
            return StatusCode(response.StatusCode, response);
        }

        //// LOGIN
        //[HttpPost("login")]
        //public async Task<IActionResult> Login([FromBody] LoginRequest request)
        //{
        //    var response = await _userService.LoginAsync(request);
        //    return StatusCode(response.StatusCode, response);
        //}

        // CREATE USER (SIGNUP)
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] SignupRequest request)
        {
            var response = await _userService.CreateUserAsync(request);
            return StatusCode(response.StatusCode, response);
        }

        // UPDATE USER
        [Authorize]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateRequest user)
        {
            var response = await _userService.UpdateUserAsync(user, id);
            return StatusCode(response.StatusCode, response);
        }

        // DELETE USER (SOFT DELETE)
        [Authorize]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var response = await _userService.DeleteUserAsync(id);
            return StatusCode(response.StatusCode, response);
        }
    }
}