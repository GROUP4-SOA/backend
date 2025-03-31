using Bookstore.Application.Dtos;

namespace Bookstore.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<UserDto> LoginAsync(LoginRequestDto loginRequest);
    }
}