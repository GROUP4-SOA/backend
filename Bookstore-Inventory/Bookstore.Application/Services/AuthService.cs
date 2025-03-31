using Bookstore.Application.Dtos;
using Bookstore.Application.Interfaces.Services;
using Bookstore.Domain.Entities;
using Bookstore.Infrastructure.Interfaces;
using Bookstore.Infrastructure.Interfaces.Repositories;

namespace Bookstore.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;

        public AuthService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<UserDto> LoginAsync(LoginRequestDto loginRequest)
        {
            var users = await _userRepository.GetAllUsersAsync();
            var user = users.FirstOrDefault(u => u.Username == loginRequest.Username);

            if (user == null)
            {
                return null;
            }

            return new UserDto
            {
                UserId = user.UserId,
                Username = user.Username,
                Role = user.Role.ToString()
            };
        }
    }
}