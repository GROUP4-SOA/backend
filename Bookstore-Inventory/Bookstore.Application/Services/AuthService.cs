using Bookstore.Application.Dtos;
using Bookstore.Application.Interfaces.Services;
using Bookstore.Domain.Entities;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;

namespace Bookstore.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IMongoCollection<User> _usersCollection;

        public AuthService(IMongoDatabase database)
        {
            _usersCollection = database.GetCollection<User>("User");
        }

        public async Task<UserDto> LoginAsync(LoginRequestDto loginRequest)
        {
            var user = await _usersCollection
                .Find(u => u.Username == loginRequest.Username && u.Password == loginRequest.Password)
                .FirstOrDefaultAsync();

            if (user == null || !user.IsActive)
            {
                throw new UnauthorizedAccessException("Tên đăng nhập hoặc mật khẩu không đúng hoặc tài khoản bị khóa.");
            }

            return MapToUserDto(user);
        }

        public async Task DeactivateUserAsync(string userId, string? currentUserId)
        {
            // Thay đổi cách tìm user dựa trên UserId string
            var filter = Builders<User>.Filter.Eq(u => u.UserId, userId);
            var userToDeactivate = await _usersCollection.Find(filter).FirstOrDefaultAsync();
        
            if (userToDeactivate == null)
            {
                throw new ArgumentException("User không tồn tại.");
            }

            var updateDefinition = Builders<User>.Update.Set(u => u.IsActive, false);
            await _usersCollection.UpdateOneAsync(filter, updateDefinition);
        }

        public async Task<UserDto> UpdateUserAsync(string userId, UpdateUserDto updateUser, string? currentUserId)
        {
            // Thay đổi cách tìm user dựa trên UserId string
            var filter = Builders<User>.Filter.Eq(u => u.UserId, userId);
            var targetUser = await _usersCollection.Find(filter).FirstOrDefaultAsync();
        
            if (targetUser == null)
            {
                throw new KeyNotFoundException("Người dùng cần cập nhật không tồn tại.");
            }

            var updateDefinition = Builders<User>.Update
                .Set(u => u.FullName, updateUser.FullName)
                .Set(u => u.Email, updateUser.Email)
                .Set(u => u.PhoneNo, updateUser.PhoneNo);

            if (!string.IsNullOrEmpty(updateUser.Password))
            {
                updateDefinition = updateDefinition.Set(u => u.Password, updateUser.Password);
            }

            await _usersCollection.UpdateOneAsync(filter, updateDefinition);
            var updatedUser = await _usersCollection.Find(filter).FirstOrDefaultAsync();
            return MapToUserDto(updatedUser);
        }

        public async Task<List<UserDto>> GetAllUsersAsync(string? currentUserId)
        {
            // Tạm thời bỏ qua việc kiểm tra quyền admin
            // Trong thực tế, bạn nên implement một hệ thống authentication/authorization đầy đủ
            var users = await _usersCollection.Find(_ => true).ToListAsync();
            return users.Select(MapToUserDto).ToList();
        }


        private UserDto MapToUserDto(User user)
        {
            return new UserDto
            {
                UserId = user.UserId,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNo = user.PhoneNo,
                Role = user.Role,
                IsActive = user.IsActive
            };
        }
    }
}
