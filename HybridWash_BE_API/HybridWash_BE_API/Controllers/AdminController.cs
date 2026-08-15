using HybridWash.Services.DTOs;
using HybridWash.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HybridWash_BE_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AdminController(IAuthService authService)
        {
            _authService = authService;
        }

        // Tạm thời để [AllowAnonymous] để tiện test trên Swagger
        [AllowAnonymous]
        [HttpPost("staff")]
        public async Task<IActionResult> CreateStaff([FromBody] CreateStaffRequestDTO request)
        {
            try
            {
                var response = await _authService.CreateStaffAsync(request);
                return Ok(new { Success = true, Message = "Tạo tài khoản Staff thành công.", Data = response });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var users = await _authService.GetAllUsersAsync();
                return Ok(new { Success = true, Data = users });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
