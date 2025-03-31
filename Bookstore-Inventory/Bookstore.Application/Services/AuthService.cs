// Bookstore.Application/Services/AuthService.cs
using Bookstore.Application.DTOs;
using Bookstore.Application.Interfaces.Services;
using Bookstore.Infrastructure.Interfaces.Repositories;
using Bookstore.Domain.Entities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Bookstore.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly string _jwtSecret = "YourSecretKeyHere1234567890"; // Thay bằng secret key trong appsettings.json

        public AuthService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<UserDto> LoginAsync(LoginRequestDto loginDto)
        {
            var user = await _userRepository.GetByUsernameAsync(loginDto.Username);
            if (user == null || user.Password != loginDto.Password) // Nên mã hóa mật khẩu trong thực tế
                throw new Exception("Invalid username or password");

            var userDto = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Role = user.Role
            };
            userDto.Token = GenerateJwtToken(userDto);
            return userDto;
        }

        public async Task<UserDto> RegisterAsync(UserDto userDto)
        {
            var existingUser = await _userRepository.GetByUsernameAsync(userDto.Username);
            if (existingUser != null)
                throw new Exception("Username already exists");

            var user = new User
            {
                Id = Guid.NewGuid().ToString(),
                Username = userDto.Username,
                Password = userDto.Password, // Nên mã hóa mật khẩu
                Role = userDto.Role ?? "User"
            };
            await _userRepository.AddAsync(user);

            var result = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Role = user.Role
            };
            result.Token = GenerateJwtToken(result);
            return result;
        }

        public string GenerateJwtToken(UserDto user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSecret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}