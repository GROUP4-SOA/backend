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
            var user = await _usersCollection
                .Find(u => u.Username == loginRequest.Username && u.Password == loginRequest.Password)
                .FirstOrDefaultAsync();

            if (user == null)
                throw new ArgumentException("Tên đăng nhập hoặc mật khẩu không đúng.");

            if (!user.IsActive)
                throw new ArgumentException("Tài khoản đã bị khóa.");

            return MapToUserDto(user);
        }

        public async Task<UserDto> UpdateUserAsync(string userId, UpdateUserDto updateUser, string currentUsername)
        {
            // Tìm user hiện tại dựa trên currentUsername để xác định vai trò
            var currentUser = await _usersCollection
                .Find(u => u.Username == currentUsername)
                .FirstOrDefaultAsync();

            if (currentUser == null)
                throw new ArgumentException("Người dùng hiện tại không tồn tại.");

            // Chỉ ADMIN được phép cập nhật
            if (currentUser.Role != UserRole.ADMINISTRATOR)
                throw new ArgumentException("Chỉ ADMINISTRATOR mới có quyền cập nhật thông tin tài khoản.");

            // Tìm user cần cập nhật
            var targetUser = await _usersCollection
                .Find(u => u.UserId == userId)
                .FirstOrDefaultAsync();

            if (targetUser == null)
                throw new ArgumentException("Người dùng cần cập nhật không tồn tại.");

            // Cập nhật tất cả thông tin (bao gồm mật khẩu nếu có)
            targetUser.FullName = updateUser.FullName;
            targetUser.Email = updateUser.Email;
            targetUser.PhoneNo = updateUser.PhoneNo;
            if (!string.IsNullOrEmpty(updateUser.Password))
            {
                targetUser.Password = updateUser.Password;
            }

            await _usersCollection.ReplaceOneAsync(u => u.UserId == userId, targetUser);
            return MapToUserDto(targetUser);
        }

        public async Task DeleteUserAsync(string userId, string currentUsername)
        {
            // Tìm user hiện tại để kiểm tra vai trò
            var currentUser = await _usersCollection
                .Find(u => u.Username == currentUsername)
                .FirstOrDefaultAsync();

            if (currentUser == null)
                throw new ArgumentException("Người dùng hiện tại không tồn tại.");

            // Chỉ ADMIN được phép xóa
            if (currentUser.Role != UserRole.ADMINISTRATOR)
                throw new ArgumentException("Chỉ ADMINISTRATOR mới có quyền xóa tài khoản.");

            var targetUser = await _usersCollection
                .Find(u => u.UserId == userId)
                .FirstOrDefaultAsync();

            if (targetUser == null)
                throw new ArgumentException("Người dùng cần xóa không tồn tại.");

            await _usersCollection.DeleteOneAsync(u => u.UserId == userId);
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