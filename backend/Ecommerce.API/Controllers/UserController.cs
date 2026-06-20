using Ecommerce.Contracts.Services;
using Ecommerce.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Ecommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ICurrentUserService _currentUserService;

        public UserController(IUserService userService, ICurrentUserService currentUserService)
        {
            _userService = userService;
            _currentUserService = currentUserService;
        }

        [Authorize(Roles = "Customer,Admin")]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            int userId = _currentUserService.UserId;
            var result = await _userService.GetUserDetails(userId);
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserDetails(int userId)
        {
            var result = await _userService.GetUserDetails(userId);
            return Ok(result);
        }

        [Authorize(Roles = "Customer,Admin")]
        [HttpPut("updateProfile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UserProfileRequest request)
        {
            var result = await _userService.UpdateProfile(request);
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("listUsers")]
        public async Task<IActionResult> ListUsers([FromQuery] PageRequest query)
        {
            var result = await _userService.ListUsers(query);
            return Ok(result);
        }

        [HttpPost("changePassword")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var result = await _userService.ChangePassword(request);
            return Ok(result);
        }

        [Authorize(Roles = "Customer")]    
        [HttpPost("ToggleAccount")]
        public async Task<IActionResult> ToggleAccountStatus()
        {
            var result = await _userService.ToggleAccountStatus();
            return Ok(result);
        }

        [Authorize(Roles = "Customer")]
        [HttpPost("UserAddress")]
        public async Task<IActionResult> AddUserAddress(AddAddressRequest request)
        {
            var result = await _userService.AddUserAddress(request);
            return Ok(result);
        }

        [Authorize(Roles = "Customer")]
        [HttpGet("AllUserAdress")]
        public async Task<IActionResult> GetAllUserAddress()
        {
            var result = await _userService.GetAllUserAddress();
            return Ok(result);
        }

        [Authorize(Roles ="Admin")]
        [HttpGet("RevokeAdmin")]
        public async Task<IActionResult> RevokeAdmin(int userId)
        {
            var result = await _userService.RevokeAdmin(userId);
            return Ok(result);
        }
    }
}
