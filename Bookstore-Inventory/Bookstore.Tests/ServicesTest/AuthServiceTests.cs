using Bookstore.Application.Dtos;
using Bookstore.Application.Services;
using Bookstore.Domain.Entities;
using MongoDB.Driver;
using Moq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Bookstore.Tests
{
    public class AuthServiceTests
    {
        private readonly Mock<IMongoCollection<User>> _usersCollectionMock;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            var databaseMock = new Mock<IMongoDatabase>();
            _usersCollectionMock = new Mock<IMongoCollection<User>>();

            databaseMock.Setup(db => db.GetCollection<User>("User", null))
                .Returns(_usersCollectionMock.Object);

            _authService = new AuthService(databaseMock.Object);
        }

        // Test LoginAsync
        [Fact]
        public async Task LoginAsync_ValidCredentials_ReturnsUserDto()
        {
            var loginRequest = new LoginRequestDto { Username = "testuser", Password = "password123" };
            var user = new User
            {
                UserId = "1",
                Username = "testuser",
                Password = "password123",
                FullName = "Test User",
                Email = "test@example.com",
                PhoneNo = "123456789",
                Role = UserRole.ADMINISTRATOR,
                IsActive = true
            };

            var findFluentMock = new Mock<IAsyncCursor<User>>();
            findFluentMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<CancellationToken>())).ReturnsAsync(user);

            _usersCollectionMock.Setup(x => x.FindAsync(It.IsAny<FilterDefinition<User>>(), null, CancellationToken.None))
                .ReturnsAsync(findFluentMock.Object);

            var result = await _authService.LoginAsync(loginRequest);

            Assert.NotNull(result);
            Assert.Equal(user.UserId, result.UserId);
            Assert.Equal(user.Username, result.Username);
            Assert.Equal(user.FullName, result.FullName);
            Assert.Equal(user.Email, result.Email);
            Assert.Equal(user.PhoneNo, result.PhoneNo);
            Assert.Equal(user.Role, result.Role); // Sửa lại để so sánh string
            Assert.True(result.IsActive);
        }

        [Fact]
        public async Task LoginAsync_InvalidCredentials_ThrowsUnauthorizedAccessException()
        {
            var loginRequest = new LoginRequestDto { Username = "testuser", Password = "wrongpassword" };
            var findFluentMock = new Mock<IAsyncCursor<User>>();
            findFluentMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<CancellationToken>())).ReturnsAsync((User)null);

            _usersCollectionMock.Setup(x => x.FindAsync(It.IsAny<FilterDefinition<User>>(), null, CancellationToken.None))
                .ReturnsAsync(findFluentMock.Object);

            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authService.LoginAsync(loginRequest));
            Assert.Equal("Tên đăng nhập hoặc mật khẩu không đúng hoặc tài khoản bị khóa.", exception.Message);
        }

        [Fact]
        public async Task LoginAsync_InactiveUser_ThrowsUnauthorizedAccessException()
        {
            var loginRequest = new LoginRequestDto { Username = "testuser", Password = "password123" };
            var user = new User { Username = "testuser", Password = "password123", IsActive = false };

            var findFluentMock = new Mock<IAsyncCursor<User>>();
            findFluentMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<CancellationToken>())).ReturnsAsync(user);

            _usersCollectionMock.Setup(x => x.FindAsync(It.IsAny<FilterDefinition<User>>(), null, CancellationToken.None))
                .ReturnsAsync(findFluentMock.Object);

            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authService.LoginAsync(loginRequest));
            Assert.Equal("Tên đăng nhập hoặc mật khẩu không đúng hoặc tài khoản bị khóa.", exception.Message);
        }

        // Test UpdateUserAsync
        [Fact]
        public async Task UpdateUserAsync_AdminUpdatesUser_SuccessfullyUpdates()
        {
            var userId = "2";
            var currentUserId = "1";
            var updateUserDto = new UpdateUserDto
            {
                FullName = "Updated Name",
                Email = "updated@example.com",
                PhoneNo = "987654321",
                Password = "newpassword"
            };
            var currentUser = new User { UserId = currentUserId, Role = UserRole.ADMINISTRATOR, IsActive = true };
            var targetUser = new User { UserId = userId, Username = "target", IsActive = true };
            var updatedUser = new User
            {
                UserId = userId,
                Username = "target",
                FullName = updateUserDto.FullName,
                Email = updateUserDto.Email,
                PhoneNo = updateUserDto.PhoneNo,
                Password = updateUserDto.Password,
                IsActive = true
            };

            var findCurrentUserMock = new Mock<IAsyncCursor<User>>();
            findCurrentUserMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<CancellationToken>())).ReturnsAsync(currentUser);
            var findTargetUserMock = new Mock<IAsyncCursor<User>>();
            findTargetUserMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<CancellationToken>())).ReturnsAsync(targetUser);
            var findUpdatedUserMock = new Mock<IAsyncCursor<User>>();
            findUpdatedUserMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<CancellationToken>())).ReturnsAsync(updatedUser);

            _usersCollectionMock.SetupSequence(x => x.FindAsync(It.IsAny<FilterDefinition<User>>(), null, CancellationToken.None))
                .ReturnsAsync(findCurrentUserMock.Object)
                .ReturnsAsync(findTargetUserMock.Object)
                .ReturnsAsync(findUpdatedUserMock.Object);

            _usersCollectionMock.Setup(x => x.UpdateOneAsync(It.IsAny<FilterDefinition<User>>(), It.IsAny<UpdateDefinition<User>>(), null, CancellationToken.None))
                .ReturnsAsync(new UpdateResult.Acknowledged(1, 1, null));

            var result = await _authService.UpdateUserAsync(userId, updateUserDto, currentUserId);

            Assert.NotNull(result);
            Assert.Equal(updatedUser.UserId, result.UserId);
            Assert.Equal(updatedUser.FullName, result.FullName);
            Assert.Equal(updatedUser.Email, result.Email);
            Assert.Equal(updatedUser.PhoneNo, result.PhoneNo);
        }

        [Fact]
        public async Task UpdateUserAsync_NonAdmin_ThrowsUnauthorizedAccessException()
        {
            var userId = "2";
            var currentUserId = "1";
            var updateUserDto = new UpdateUserDto { FullName = "Updated Name" };
            var currentUser = new User { UserId = currentUserId, Role = UserRole.STAFF, IsActive = true }; // Sửa thành STAFF thay vì ADMIN

            var findCurrentUserMock = new Mock<IAsyncCursor<User>>();
            findCurrentUserMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<CancellationToken>())).ReturnsAsync(currentUser);

            _usersCollectionMock.Setup(x => x.FindAsync(It.IsAny<FilterDefinition<User>>(), null, CancellationToken.None))
                .ReturnsAsync(findCurrentUserMock.Object);

            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _authService.UpdateUserAsync(userId, updateUserDto, currentUserId));
            Assert.Equal("Chỉ ADMINISTRATOR mới có quyền cập nhật tài khoản.", exception.Message);
        }

        [Fact]
        public async Task UpdateUserAsync_UserNotFound_ThrowsKeyNotFoundException()
        {
            var userId = "2";
            var currentUserId = "1";
            var updateUserDto = new UpdateUserDto { FullName = "Updated Name" };
            var currentUser = new User { UserId = currentUserId, Role = UserRole.ADMINISTRATOR, IsActive = true };

            var findCurrentUserMock = new Mock<IAsyncCursor<User>>();
            findCurrentUserMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<CancellationToken>())).ReturnsAsync(currentUser);
            var findTargetUserMock = new Mock<IAsyncCursor<User>>();
            findTargetUserMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<CancellationToken>())).ReturnsAsync((User)null);

            _usersCollectionMock.SetupSequence(x => x.FindAsync(It.IsAny<FilterDefinition<User>>(), null, CancellationToken.None))
                .ReturnsAsync(findCurrentUserMock.Object)
                .ReturnsAsync(findTargetUserMock.Object);

            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _authService.UpdateUserAsync(userId, updateUserDto, currentUserId));
            Assert.Equal("Người dùng cần cập nhật không tồn tại.", exception.Message);
        }

        // Test DeactivateUserAsync
        [Fact]
        public async Task DeactivateUserAsync_AdminDeactivatesUser_SuccessfullyDeactivates()
        {
            var userId = "2";
            var currentUserId = "1";
            var currentUser = new User { UserId = currentUserId, Role = UserRole.ADMINISTRATOR, IsActive = true };
            var targetUser = new User { UserId = userId, IsActive = true };
            var updatedUser = new User { UserId = userId, IsActive = false };

            var findTargetUserMock = new Mock<IAsyncCursor<User>>();
            findTargetUserMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<CancellationToken>())).ReturnsAsync(targetUser);
            var findCurrentUserMock = new Mock<IAsyncCursor<User>>();
            findCurrentUserMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<CancellationToken>())).ReturnsAsync(currentUser);
            var findUpdatedUserMock = new Mock<IAsyncCursor<User>>();
            findUpdatedUserMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<CancellationToken>())).ReturnsAsync(updatedUser);

            _usersCollectionMock.SetupSequence(x => x.FindAsync(It.IsAny<FilterDefinition<User>>(), null, CancellationToken.None))
                .ReturnsAsync(findTargetUserMock.Object)
                .ReturnsAsync(findCurrentUserMock.Object)
                .ReturnsAsync(findUpdatedUserMock.Object);

            _usersCollectionMock.Setup(x => x.UpdateOneAsync(It.IsAny<FilterDefinition<User>>(), It.IsAny<UpdateDefinition<User>>(), null, CancellationToken.None))
                .ReturnsAsync(new UpdateResult.Acknowledged(1, 1, null));

            await _authService.DeactivateUserAsync(userId, currentUserId);

            var result = await _usersCollectionMock.Object.Find(u => u.UserId == userId).FirstOrDefaultAsync();
            Assert.False(result.IsActive);
        }

        [Fact]
        public async Task DeactivateUserAsync_NonAdminNonSelf_ThrowsUnauthorizedAccessException()
        {
            var userId = "2";
            var currentUserId = "1";
            var currentUser = new User { UserId = currentUserId, Role = UserRole.STAFF, IsActive = true };
            var targetUser = new User { UserId = userId, IsActive = true };

            var findTargetUserMock = new Mock<IAsyncCursor<User>>();
            findTargetUserMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<CancellationToken>())).ReturnsAsync(targetUser);
            var findCurrentUserMock = new Mock<IAsyncCursor<User>>();
            findCurrentUserMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<CancellationToken>())).ReturnsAsync(currentUser);

            _usersCollectionMock.SetupSequence(x => x.FindAsync(It.IsAny<FilterDefinition<User>>(), null, CancellationToken.None))
                .ReturnsAsync(findTargetUserMock.Object)
                .ReturnsAsync(findCurrentUserMock.Object);

            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _authService.DeactivateUserAsync(userId, currentUserId));
            Assert.Equal("Bạn không có quyền vô hiệu hóa user này.", exception.Message);
        }

        [Fact]
        public async Task DeactivateUserAsync_UserNotFound_ThrowsArgumentException()
        {
            var userId = "2";
            var currentUserId = "1";

            var findTargetUserMock = new Mock<IAsyncCursor<User>>();
            findTargetUserMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<CancellationToken>())).ReturnsAsync((User)null);

            _usersCollectionMock.Setup(x => x.FindAsync(It.IsAny<FilterDefinition<User>>(), null, CancellationToken.None))
                .ReturnsAsync(findTargetUserMock.Object);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _authService.DeactivateUserAsync(userId, currentUserId));
            Assert.Equal("User không tồn tại.", exception.Message);
        }

        // Test GetAllUsersAsync
        [Fact]
        public async Task GetAllUsersAsync_Admin_ReturnsAllUsers()
        {
            var currentUserId = "1";
            var currentUser = new User { UserId = currentUserId, Role = UserRole.ADMINISTRATOR, IsActive = true };
            var users = new List<User>
            {
                new User { UserId = "1", Username = "admin", Role = UserRole.ADMINISTRATOR, IsActive = true },
                new User { UserId = "2", Username = "user", Role = UserRole.STAFF, IsActive = true }
            };

            var findCurrentUserMock = new Mock<IAsyncCursor<User>>();
            findCurrentUserMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<CancellationToken>())).ReturnsAsync(currentUser);
            var findAllUsersMock = new Mock<IAsyncCursor<User>>();
            findAllUsersMock.Setup(x => x.ToListAsync(It.IsAny<CancellationToken>())).ReturnsAsync(users);

            _usersCollectionMock.SetupSequence(x => x.FindAsync(It.IsAny<FilterDefinition<User>>(), null, CancellationToken.None))
                .ReturnsAsync(findCurrentUserMock.Object)
                .ReturnsAsync(findAllUsersMock.Object);

            var result = await _authService.GetAllUsersAsync(currentUserId);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains(result, u => u.UserId == "1" && u.Role == UserRole.ADMINISTRATOR);
            Assert.Contains(result, u => u.UserId == "2" && u.Role == UserRole.STAFF);
        }

        [Fact]
        public async Task GetAllUsersAsync_NonAdmin_ThrowsUnauthorizedAccessException()
        {
            var currentUserId = "1";
            var currentUser = new User { UserId = currentUserId, Role = UserRole.STAFF, IsActive = true };

            var findCurrentUserMock = new Mock<IAsyncCursor<User>>();
            findCurrentUserMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<CancellationToken>())).ReturnsAsync(currentUser);

            _usersCollectionMock.Setup(x => x.FindAsync(It.IsAny<FilterDefinition<User>>(), null, CancellationToken.None))
                .ReturnsAsync(findCurrentUserMock.Object);

            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _authService.GetAllUsersAsync(currentUserId));
            Assert.Equal("Bạn không có quyền xem danh sách tài khoản.", exception.Message);
        }
    }
}