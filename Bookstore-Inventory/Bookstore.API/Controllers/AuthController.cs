using Bookstore.Application.Dtos;
using Bookstore.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Bookstore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequest)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var user = await _authService.LoginAsync(loginRequest);
                return Ok(user);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống", error = ex.Message });
            }
        }

        [HttpPut("update/{userId}")]
        public async Task<IActionResult> Update(string userId, [FromBody] UpdateUserDto updateUser)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                // Lấy Username từ header
                var currentUsername = Request.Headers["X-Username"];
                if (string.IsNullOrEmpty(currentUsername))
                    return BadRequest(new { message = "Yêu cầu cung cấp X-Username trong header." });

                var user = await _authService.UpdateUserAsync(userId, updateUser, currentUsername);
                return Ok(user);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống", error = ex.Message });
            }
        }

        [HttpDelete("{userId}")]
        public async Task<IActionResult> Delete(string userId)
        {
            try
            {
                // Lấy Username từ header
                var currentUsername = Request.Headers["X-Username"];
                if (string.IsNullOrEmpty(currentUsername))
                    return BadRequest(new { message = "Yêu cầu cung cấp X-Username trong header." });

                await _authService.DeleteUserAsync(userId, currentUsername);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống", error = ex.Message });
            }
        }
    }
}