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
            Console.WriteLine("Received login request.");
            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState is invalid for login request.");
                return BadRequest(ModelState);
            }

            try
            {
                var user = await _authService.LoginAsync(loginRequest);
                Console.WriteLine("Login successful.");
                return Ok(user);
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"ArgumentException in Login: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in Login: {ex.Message}");
                return StatusCode(500, new { message = "Lỗi hệ thống", error = ex.Message });
            }
        }

        [HttpPut("update/{userId}")]
        public async Task<IActionResult> Update(string userId, [FromBody] UpdateUserDto updateUser)
        {
            Console.WriteLine($"Received request to update userId: {userId}");
            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState is invalid for update request.");
                return BadRequest(ModelState);
            }

            // Validation thủ công
            if (string.IsNullOrWhiteSpace(updateUser.FullName) ||
                string.IsNullOrWhiteSpace(updateUser.Email) ||
                string.IsNullOrWhiteSpace(updateUser.PhoneNo))
            {
                Console.WriteLine("Validation failed: FullName, Email, or PhoneNo is empty.");
                return BadRequest(new { message = "FullName, Email, và PhoneNo là bắt buộc." });
            }

            try
            {
                var currentUsername = Request.Headers["X-Username"];
                Console.WriteLine($"X-Username: {currentUsername}");
                if (string.IsNullOrEmpty(currentUsername))
                {
                    Console.WriteLine("X-Username is missing.");
                    return BadRequest(new { message = "Yêu cầu cung cấp X-Username trong header." });
                }

                var user = await _authService.UpdateUserAsync(userId, updateUser, currentUsername);
                Console.WriteLine("User updated successfully.");
                return Ok(user);
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"ArgumentException in Update: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in Update: {ex.Message}");
                return StatusCode(500, new { message = "Lỗi hệ thống", error = ex.Message });
            }
        }

        [HttpDelete("{userId}")]
        public async Task<IActionResult> Delete(string userId)
        {
            Console.WriteLine($"Received request to delete userId: {userId}");
            try
            {
                var currentUsername = Request.Headers["X-Username"];
                Console.WriteLine($"X-Username: {currentUsername}");
                if (string.IsNullOrEmpty(currentUsername))
                {
                    Console.WriteLine("X-Username is missing.");
                    return BadRequest(new { message = "Yêu cầu cung cấp X-Username trong header." });
                }

                await _authService.DeleteUserAsync(userId, currentUsername);
                Console.WriteLine("User deleted successfully.");
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"ArgumentException in Delete: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in Delete: {ex.Message}");
                return StatusCode(500, new { message = "Lỗi hệ thống", error = ex.Message });
            }
        }
    }
}