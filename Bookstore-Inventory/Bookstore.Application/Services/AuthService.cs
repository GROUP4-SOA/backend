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
            Console.WriteLine("Initializing AuthService...");
            _usersCollection = database.GetCollection<User>("User"); // Đã sửa từ "Users" thành "User"
            Console.WriteLine("AuthService initialized with User collection.");
        }

        public async Task<UserDto> LoginAsync(LoginRequestDto loginRequest)
        {
            Console.WriteLine($"Logging in user with username: {loginRequest.Username}");
            var user = await _usersCollection
                .Find(u => u.Username == loginRequest.Username && u.Password == loginRequest.Password)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                Console.WriteLine("User not found or password incorrect.");
                throw new ArgumentException("Tên đăng nhập hoặc mật khẩu không đúng.");
            }

            if (!user.IsActive)
            {
                Console.WriteLine("User account is locked.");
                throw new ArgumentException("Tài khoản đã bị khóa.");
            }

            Console.WriteLine("User logged in successfully.");
            return MapToUserDto(user);
        }

        public async Task<UserDto> UpdateUserAsync(string userId, UpdateUserDto updateUser, string currentUsername)
        {
            Console.WriteLine($"Finding user with username: {currentUsername}");
            var currentUser = await _usersCollection
                .Find(u => u.Username == currentUsername)
                .FirstOrDefaultAsync();

            if (currentUser == null)
            {
                Console.WriteLine("Current user not found.");
                throw new ArgumentException("Người dùng hiện tại không tồn tại.");
            }

            if (currentUser.Role != UserRole.ADMINISTRATOR)
            {
                Console.WriteLine("User is not an ADMIN.");
                throw new ArgumentException("Chỉ ADMINISTRATOR mới có quyền cập nhật thông tin tài khoản.");
            }

            Console.WriteLine($"Finding target user with userId: {userId}");
            var targetUser = await _usersCollection
                .Find(u => u.UserId == userId)
                .FirstOrDefaultAsync();

            if (targetUser == null)
            {
                Console.WriteLine("Target user not found.");
                throw new ArgumentException("Người dùng cần cập nhật không tồn tại.");
            }

            targetUser.FullName = updateUser.FullName;
            targetUser.Email = updateUser.Email;
            targetUser.PhoneNo = updateUser.PhoneNo;
            if (!string.IsNullOrEmpty(updateUser.Password))
            {
                Console.WriteLine("Updating password.");
                targetUser.Password = updateUser.Password;
            }

            Console.WriteLine("Updating user in MongoDB...");
            await _usersCollection.ReplaceOneAsync(u => u.UserId == userId, targetUser);
            Console.WriteLine("User updated in MongoDB.");
            return MapToUserDto(targetUser);
        }

        public async Task DeleteUserAsync(string userId, string currentUsername)
        {
            Console.WriteLine($"Finding user with username: {currentUsername}");
            var currentUser = await _usersCollection
                .Find(u => u.Username == currentUsername)
                .FirstOrDefaultAsync();

            if (currentUser == null)
            {
                Console.WriteLine("Current user not found.");
                throw new ArgumentException("Người dùng hiện tại không tồn tại.");
            }

            if (currentUser.Role != UserRole.ADMINISTRATOR)
            {
                Console.WriteLine("User is not an ADMIN.");
                throw new ArgumentException("Chỉ ADMINISTRATOR mới có quyền xóa tài khoản.");
            }

            Console.WriteLine($"Finding target user with userId: {userId}");
            var targetUser = await _usersCollection
                .Find(u => u.UserId == userId)
                .FirstOrDefaultAsync();

            if (targetUser == null)
            {
                Console.WriteLine("Target user not found.");
                throw new ArgumentException("Người dùng cần xóa không tồn tại.");
            }

            Console.WriteLine("Deleting user from MongoDB...");
            await _usersCollection.DeleteOneAsync(u => u.UserId == userId);
            Console.WriteLine("User deleted from MongoDB.");
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