using Bookstore.Application.Dtos;
using Bookstore.Application.Interfaces.Services;
using Bookstore.Application.Services;
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
                var currentUserId = Request.Headers["currentUserId"].ToString(); // Thay X-Username thành currentUserId
                Console.WriteLine($"currentUserId: {currentUserId}");
                if (string.IsNullOrEmpty(currentUserId))
                {
                    Console.WriteLine("currentUserId is missing.");
                    return BadRequest(new { message = "Yêu cầu cung cấp currentUserId trong header." });
                }

                var user = await _authService.UpdateUserAsync(userId, updateUser, currentUserId); // Dùng currentUserId
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

        [HttpDelete("deactivate/{userId}")]
        public async Task<IActionResult> DeactivateUser(string userId)
        {
            Console.WriteLine($"Received request to deactivate userId: {userId}");
            try
            {
                var currentUsername = Request.Headers["X-Username"];
                Console.WriteLine($"X-Username: {currentUsername}");
                if (string.IsNullOrEmpty(currentUsername))
                {
                    Console.WriteLine("X-Username is missing.");
                    return BadRequest(new { message = "Yêu cầu cung cấp X-Username trong header." });
                }

                await _authService.DeactivateUserAsync(userId, currentUsername);
                Console.WriteLine("User deactivated successfully.");
                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid();
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi vô hiệu hóa user: {ex.Message}");
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi vô hiệu hóa user." });
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            Console.WriteLine("Received request to get all users.");

            // Lấy userId của người gọi từ header
            var currentUsername = Request.Headers["X-Username"].ToString();
            if (string.IsNullOrEmpty(currentUsername))
            {
                Console.WriteLine("X-Username is missing.");
                return BadRequest(new { message = "Yêu cầu cung cấp X-Username trong header." });
            }

            try
            {
                var users = await _authService.GetAllUsersAsync(currentUsername);
                return Ok(users);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi lấy danh sách user: {ex.Message}");
                return StatusCode(500, new { message = "Lỗi hệ thống", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto newUserDto)
        {
            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState is invalid.");
                return BadRequest(ModelState);
            }

            try
            {
                var createdUser = await _authService.CreateUserAsync(newUserDto); // Passing CreateUserDto
                Console.WriteLine("User created successfully.");
                return Ok(createdUser);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi tạo user: {ex.Message}");
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi tạo user.", error = ex.Message });
            }
        }


    }
}