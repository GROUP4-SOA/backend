using Bookstore.Application.Dtos;
using Bookstore.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Bookstore.API.Controllers
{
    [Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi trong quá trình đăng nhập");
            return StatusCode(500, new { message = "Có lỗi xảy ra" });
        }
    }

    [HttpPut("{userId}")]
    public async Task<IActionResult> Update(string userId, [FromBody] UpdateUserDto updateUser)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (string.IsNullOrEmpty(userId))
            return BadRequest(new { message = "UserId không hợp lệ" });

        try
        {
            var currentUser = User.Identity?.Name;
            var user = await _authService.UpdateUserAsync(userId, updateUser, currentUser);
            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Lỗi khi cập nhật user {userId}");
            return StatusCode(500, new { message = "Có lỗi xảy ra" });
        }
    }

    [HttpDelete("{userId}")]
    public async Task<IActionResult> Delete(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            return BadRequest(new { message = "UserId không hợp lệ" });

        try
        {
            var currentUser = User.Identity?.Name;
            await _authService.DeleteUserAsync(userId, currentUser);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Lỗi khi xóa user {userId}");
            return StatusCode(500, new { message = "Có lỗi xảy ra" });
        }
    }
}
}