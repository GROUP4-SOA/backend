using Bookstore.Application.Dtos;
using Bookstore.Application.Interfaces.Services;
using Bookstore.Domain.Entities;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace Bookstore.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IMongoCollection<User> _usersCollection;

        public AuthService(IMongoDatabase database)
        {
            _usersCollection = database.GetCollection<User>("Users");
        }

        public async Task<UserDto> LoginAsync(LoginRequestDto loginRequest)
        {
            var user = await _usersCollection.Find(u => u.Username == loginRequest.Username && u.Password == loginRequest.Password).FirstOrDefaultAsync();
            if (user == null)
                throw new ArgumentException("Tên đăng nhập hoặc mật khẩu không đúng");

            return new UserDto
            {
                UserId = user.UserId,
                Username = user.Username,
                Password = user.Password,
                Email = user.Email,
                Role = user.Role,
            };
        }
    }
}