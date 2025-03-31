// Bookstore.Application/Interfaces/Services/IAuthService.cs
using Bookstore.Application.DTOs;

namespace Bookstore.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<UserDto> LoginAsync(LoginRequestDto loginDto);
        Task<UserDto> RegisterAsync(UserDto userDto);
        string GenerateJwtToken(UserDto user);
    }
}