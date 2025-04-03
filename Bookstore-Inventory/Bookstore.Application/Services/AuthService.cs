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

        public async Task<UserDto> UpdateUserAsync(string userId, UpdateUserDto updateUser, string currentUserId)
        {
            var currentUser = await _usersCollection.Find(u => u.UserId == currentUserId).FirstOrDefaultAsync();
            if (currentUser == null || currentUser.Role != UserRole.ADMINISTRATOR)
            {
                throw new UnauthorizedAccessException("Chỉ ADMINISTRATOR mới có quyền cập nhật tài khoản.");
            }

            var targetUser = await _usersCollection.Find(u => u.UserId == userId).FirstOrDefaultAsync();
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

            await _usersCollection.UpdateOneAsync(u => u.UserId == userId, updateDefinition);
            var updatedUser = await _usersCollection.Find(u => u.UserId == userId).FirstOrDefaultAsync();
            return MapToUserDto(updatedUser);
        }

        public async Task DeactivateUserAsync(string userId, string currentUserId)
        {
            var userToDeactivate = await _usersCollection.Find(u => u.UserId == userId).FirstOrDefaultAsync();
            if (userToDeactivate == null)
            {
                throw new ArgumentException("User không tồn tại.");
            }

            // Kiểm tra quyền xóa, chỉ admin hoặc chính chủ mới có thể vô hiệu hóa tài khoản
            var currentUser = await _usersCollection.Find(u => u.UserId == currentUserId).FirstOrDefaultAsync();
            if (currentUser == null || (currentUser.UserId != userId && currentUser.Role != UserRole.ADMINISTRATOR))
            {
                throw new UnauthorizedAccessException("Bạn không có quyền vô hiệu hóa user này.");
            }

            var updateDefinition = Builders<User>.Update.Set(u => u.IsActive, false);
            await _usersCollection.UpdateOneAsync(u => u.UserId == userId, updateDefinition);
        }

        public async Task<List<UserDto>> GetAllUsersAsync(string currentUserId)
        {
            var currentUser = await _usersCollection.Find(u => u.UserId == currentUserId).FirstOrDefaultAsync();

            if (currentUser == null || currentUser.Role != UserRole.ADMINISTRATOR)
            {
                throw new UnauthorizedAccessException("Bạn không có quyền xem danh sách tài khoản.");
            }

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
