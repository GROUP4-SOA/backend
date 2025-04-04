using Bookstore.Application.Dtos;
using Bookstore.Application.Services;
using Bookstore.Domain.Entities;
using MongoDB.Driver;
using Moq;
using System;
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
            // Arrange
            var loginRequest = new LoginRequestDto
            {
                Username = "tranthimai",
                Password = "nbejhkj12g"
            };
            var user = new User
            {
                UserId = "67eacf021af224f20dca689e",
                Username = "tranthimai",
                Email = "ttm@gmail.com",
                PhoneNo = "09213972131",
                Password = "nbejhkj12g",
                Role = UserRole.STAFF,
                IsActive = true,
                FullName = "tanthimai"
            };

            var mockCursor = new Mock<IAsyncCursor<User>>();
            mockCursor.Setup(_ => _.Current).Returns(new List<User> { user });
            mockCursor
                .SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            _usersCollectionMock
                .Setup(x => x.FindAsync(
                    It.IsAny<FilterDefinition<User>>(),
                    It.IsAny<FindOptions<User, User>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            // Act
            var result = await _authService.LoginAsync(loginRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.UserId, result.UserId);
            Assert.Equal(user.Username, result.Username);
            Assert.Equal(user.Email, result.Email);
            Assert.Equal(user.PhoneNo, result.PhoneNo);
            Assert.Equal(user.Role, result.Role);
            Assert.Equal(user.IsActive, result.IsActive);
            Assert.Equal(user.FullName, result.FullName);
        }

        [Fact]
        public async Task LoginAsync_InvalidCredentials_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var loginRequest = new LoginRequestDto
            {
                Username = "tranthimai",
                Password = "wrongpassword"
            };

            var mockCursor = new Mock<IAsyncCursor<User>>();
            mockCursor.Setup(_ => _.Current).Returns(new List<User>());
            mockCursor
                .SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _usersCollectionMock
                .Setup(x => x.FindAsync(
                    It.IsAny<FilterDefinition<User>>(),
                    It.IsAny<FindOptions<User, User>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authService.LoginAsync(loginRequest));
            Assert.Equal("Tên đăng nhập hoặc mật khẩu không đúng hoặc tài khoản bị khóa.", exception.Message);
        }

        [Fact]
        public async Task LoginAsync_InactiveAccount_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var loginRequest = new LoginRequestDto
            {
                Username = "tranthimai",
                Password = "nbejhkj12g"
            };
            var user = new User
            {
                UserId = "67eacf021af224f20dca689e",
                Username = "tranthimai",
                Email = "ttm@gmail.com",
                PhoneNo = "09213972131",
                Password = "nbejhkj12g",
                Role = UserRole.STAFF,
                IsActive = false,
                FullName = "tanthimai"
            };

            var mockCursor = new Mock<IAsyncCursor<User>>();
            mockCursor.Setup(_ => _.Current).Returns(new List<User> { user });
            mockCursor
                .SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            _usersCollectionMock
                .Setup(x => x.FindAsync(
                    It.IsAny<FilterDefinition<User>>(),
                    It.IsAny<FindOptions<User, User>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authService.LoginAsync(loginRequest));
            Assert.Equal("Tên đăng nhập hoặc mật khẩu không đúng hoặc tài khoản bị khóa.", exception.Message);
        }

        // Test UpdateUserAsync
        [Fact]
        public async Task UpdateUserAsync_ValidInputByAdmin_ReturnsUpdatedUserDto()
        {
            // Arrange
            var userId = "67eacf021af224f20dca689f";
            var currentUserId = "admin-id";
            var updateUserDto = new UpdateUserDto
            {
                FullName = "Updated Le Hoang Phuc",
                Email = "updated.lehoangphuc@example.com",
                PhoneNo = "0901234567",
                Password = "new_hashed_password"
            };
            var targetUser = new User
            {
                UserId = userId,
                Username = "lehoangphuc",
                Email = "lehoangphuc@example.com",
                PhoneNo = "09003345678",
                Password = "hashed_password_3",
                Role = UserRole.STAFF,
                IsActive = false,
                FullName = "Le Hoang Phuc"
            };
            var updatedTargetUser = new User
            {
                UserId = userId,
                Username = "lehoangphuc",
                Email = updateUserDto.Email,
                PhoneNo = updateUserDto.PhoneNo,
                Password = updateUserDto.Password,
                Role = UserRole.STAFF,
                IsActive = false,
                FullName = updateUserDto.FullName
            };

            var mockCursorTargetUser = new Mock<IAsyncCursor<User>>();
            mockCursorTargetUser.Setup(_ => _.Current).Returns(new List<User> { targetUser });
            mockCursorTargetUser
                .SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            var mockCursorUpdatedUser = new Mock<IAsyncCursor<User>>();
            mockCursorUpdatedUser.Setup(_ => _.Current).Returns(new List<User> { updatedTargetUser });
            mockCursorUpdatedUser
                .SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            _usersCollectionMock
                .Setup(x => x.FindAsync(
                    It.IsAny<FilterDefinition<User>>(),
                    It.IsAny<FindOptions<User, User>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursorTargetUser.Object)
                .Callback(() => _usersCollectionMock
                    .Setup(x => x.FindAsync(
                        It.IsAny<FilterDefinition<User>>(),
                        It.IsAny<FindOptions<User, User>>(),
                        It.IsAny<CancellationToken>()))
                    .ReturnsAsync(mockCursorUpdatedUser.Object));

            _usersCollectionMock
                .Setup(x => x.UpdateOneAsync(
                    It.IsAny<FilterDefinition<User>>(),
                    It.IsAny<UpdateDefinition<User>>(),
                    It.IsAny<UpdateOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UpdateResult.Acknowledged(1, 1, null));

            // Act
            var result = await _authService.UpdateUserAsync(userId, updateUserDto, currentUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.UserId);
            Assert.Equal(updateUserDto.FullName, result.FullName);
            Assert.Equal(updateUserDto.Email, result.Email);
            Assert.Equal(updateUserDto.PhoneNo, result.PhoneNo);
            Assert.Equal(targetUser.Role, result.Role);
            Assert.Equal(targetUser.IsActive, result.IsActive);
            _usersCollectionMock.Verify(x => x.UpdateOneAsync(It.IsAny<FilterDefinition<User>>(), It.IsAny<UpdateDefinition<User>>(), It.IsAny<UpdateOptions>(), It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task UpdateUserAsync_NonAdminUser_SuccessfullyUpdatesUser()
        {
            // Arrange
            var userId = "67eacf021af224f20dca689f";
            var currentUserId = "67eacf021af224f20dca689f";
            var updateUserDto = new UpdateUserDto
            {
                FullName = "Updated Le Hoang Phuc",
                Email = "updated.lehoangphuc@example.com",
                PhoneNo = "0901234567"
            };
            var targetUser = new User
            {
                UserId = userId,
                Username = "lehoangphuc",
                Email = "lehoangphuc@example.com",
                PhoneNo = "09003345678",
                Password = "hashed_password_3",
                Role = UserRole.STAFF,
                IsActive = false,
                FullName = "Le Hoang Phuc"
            };
            var updatedTargetUser = new User
            {
                UserId = userId,
                Username = "lehoangphuc",
                Email = updateUserDto.Email,
                PhoneNo = updateUserDto.PhoneNo,
                Password = targetUser.Password,
                Role = UserRole.STAFF,
                IsActive = false,
                FullName = updateUserDto.FullName
            };

            var mockCursorTargetUser = new Mock<IAsyncCursor<User>>();
            mockCursorTargetUser.Setup(_ => _.Current).Returns(new List<User> { targetUser });
            mockCursorTargetUser
                .SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            var mockCursorUpdatedUser = new Mock<IAsyncCursor<User>>();
            mockCursorUpdatedUser.Setup(_ => _.Current).Returns(new List<User> { updatedTargetUser });
            mockCursorUpdatedUser
                .SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            _usersCollectionMock
                .Setup(x => x.FindAsync(
                    It.IsAny<FilterDefinition<User>>(),
                    It.IsAny<FindOptions<User, User>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursorTargetUser.Object)
                .Callback(() => _usersCollectionMock
                    .Setup(x => x.FindAsync(
                        It.IsAny<FilterDefinition<User>>(),
                        It.IsAny<FindOptions<User, User>>(),
                        It.IsAny<CancellationToken>()))
                    .ReturnsAsync(mockCursorUpdatedUser.Object));

            _usersCollectionMock
                .Setup(x => x.UpdateOneAsync(
                    It.IsAny<FilterDefinition<User>>(),
                    It.IsAny<UpdateDefinition<User>>(),
                    It.IsAny<UpdateOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UpdateResult.Acknowledged(1, 1, null));

            // Act
            var result = await _authService.UpdateUserAsync(userId, updateUserDto, currentUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.UserId);
            Assert.Equal(updateUserDto.FullName, result.FullName);
            Assert.Equal(updateUserDto.Email, result.Email);
            Assert.Equal(updateUserDto.PhoneNo, result.PhoneNo);
            Assert.Equal(targetUser.Role, result.Role);
            Assert.Equal(targetUser.IsActive, result.IsActive);
            _usersCollectionMock.Verify(x => x.UpdateOneAsync(It.IsAny<FilterDefinition<User>>(), It.IsAny<UpdateDefinition<User>>(), It.IsAny<UpdateOptions>(), It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task UpdateUserAsync_CurrentUserNotFound_SuccessfullyUpdatesUser()
        {
            // Arrange
            var userId = "67eacf021af224f20dca689f";
            var currentUserId = "nonexistent";
            var updateUserDto = new UpdateUserDto
            {
                FullName = "Updated Le Hoang Phuc",
                Email = "updated.lehoangphuc@example.com",
                PhoneNo = "0901234567"
            };
            var targetUser = new User
            {
                UserId = userId,
                Username = "lehoangphuc",
                Email = "lehoangphuc@example.com",
                PhoneNo = "09003345678",
                Password = "hashed_password_3",
                Role = UserRole.STAFF,
                IsActive = false,
                FullName = "Le Hoang Phuc"
            };
            var updatedTargetUser = new User
            {
                UserId = userId,
                Username = "lehoangphuc",
                Email = updateUserDto.Email,
                PhoneNo = updateUserDto.PhoneNo,
                Password = targetUser.Password,
                Role = UserRole.STAFF,
                IsActive = false,
                FullName = updateUserDto.FullName
            };

            var mockCursorTargetUser = new Mock<IAsyncCursor<User>>();
            mockCursorTargetUser.Setup(_ => _.Current).Returns(new List<User> { targetUser });
            mockCursorTargetUser
                .SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            var mockCursorUpdatedUser = new Mock<IAsyncCursor<User>>();
            mockCursorUpdatedUser.Setup(_ => _.Current).Returns(new List<User> { updatedTargetUser });
            mockCursorUpdatedUser
                .SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            _usersCollectionMock
                .Setup(x => x.FindAsync(
                    It.IsAny<FilterDefinition<User>>(),
                    It.IsAny<FindOptions<User, User>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursorTargetUser.Object)
                .Callback(() => _usersCollectionMock
                    .Setup(x => x.FindAsync(
                        It.IsAny<FilterDefinition<User>>(),
                        It.IsAny<FindOptions<User, User>>(),
                        It.IsAny<CancellationToken>()))
                    .ReturnsAsync(mockCursorUpdatedUser.Object));

            _usersCollectionMock
                .Setup(x => x.UpdateOneAsync(
                    It.IsAny<FilterDefinition<User>>(),
                    It.IsAny<UpdateDefinition<User>>(),
                    It.IsAny<UpdateOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UpdateResult.Acknowledged(1, 1, null));

            // Act
            var result = await _authService.UpdateUserAsync(userId, updateUserDto, currentUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.UserId);
            Assert.Equal(updateUserDto.FullName, result.FullName);
            Assert.Equal(updateUserDto.Email, result.Email);
            Assert.Equal(updateUserDto.PhoneNo, result.PhoneNo);
            Assert.Equal(targetUser.Role, result.Role);
            Assert.Equal(targetUser.IsActive, result.IsActive);
            _usersCollectionMock.Verify(x => x.UpdateOneAsync(It.IsAny<FilterDefinition<User>>(), It.IsAny<UpdateDefinition<User>>(), It.IsAny<UpdateOptions>(), It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task UpdateUserAsync_TargetUserNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var userId = "non-existent-id";
            var currentUserId = "admin-id";
            var updateUserDto = new UpdateUserDto
            {
                FullName = "Updated Le Hoang Phuc",
                Email = "updated.lehoangphuc@example.com",
                PhoneNo = "0901234567"
            };

            var mockCursorTargetUser = new Mock<IAsyncCursor<User>>();
            mockCursorTargetUser.Setup(_ => _.Current).Returns(new List<User>());
            mockCursorTargetUser
                .SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _usersCollectionMock
                .Setup(x => x.FindAsync(
                    It.IsAny<FilterDefinition<User>>(),
                    It.IsAny<FindOptions<User, User>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursorTargetUser.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _authService.UpdateUserAsync(userId, updateUserDto, currentUserId));
            Assert.Equal("Người dùng cần cập nhật không tồn tại.", exception.Message);
        }

        // Test DeactivateUserAsync
        [Fact]
        public async Task DeactivateUserAsync_ValidInputByAdmin_SuccessfullyDeactivatesUser()
        {
            // Arrange
            var userId = "67eacf021af224f20dca689f";
            var currentUserId = "admin-id";
            var targetUser = new User
            {
                UserId = userId,
                Username = "lehoangphuc",
                Email = "lehoangphuc@example.com",
                PhoneNo = "09003345678",
                Password = "hashed_password_3",
                Role = UserRole.STAFF,
                IsActive = true,
                FullName = "Le Hoang Phuc"
            };

            var mockCursorTargetUser = new Mock<IAsyncCursor<User>>();
            mockCursorTargetUser.Setup(_ => _.Current).Returns(new List<User> { targetUser });
            mockCursorTargetUser
                .SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            _usersCollectionMock
                .Setup(x => x.FindAsync(
                    It.IsAny<FilterDefinition<User>>(),
                    It.IsAny<FindOptions<User, User>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursorTargetUser.Object);

            _usersCollectionMock
                .Setup(x => x.UpdateOneAsync(
                    It.IsAny<FilterDefinition<User>>(),
                    It.IsAny<UpdateDefinition<User>>(),
                    It.IsAny<UpdateOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UpdateResult.Acknowledged(1, 1, null));

            // Act
            await _authService.DeactivateUserAsync(userId, currentUserId);

            // Assert
            _usersCollectionMock.Verify(x => x.UpdateOneAsync(It.IsAny<FilterDefinition<User>>(), It.IsAny<UpdateDefinition<User>>(), It.IsAny<UpdateOptions>(), It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task DeactivateUserAsync_SelfDeactivation_SuccessfullyDeactivatesUser()
        {
            // Arrange
            var userId = "67eacf021af224f20dca689f";
            var currentUserId = "67eacf021af224f20dca689f";
            var targetUser = new User
            {
                UserId = userId,
                Username = "lehoangphuc",
                Role = UserRole.STAFF,
                IsActive = true
            };

            var mockCursorTargetUser = new Mock<IAsyncCursor<User>>();
            mockCursorTargetUser.Setup(_ => _.Current).Returns(new List<User> { targetUser });
            mockCursorTargetUser
                .SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            _usersCollectionMock
                .Setup(x => x.FindAsync(
                    It.IsAny<FilterDefinition<User>>(),
                    It.IsAny<FindOptions<User, User>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursorTargetUser.Object);

            _usersCollectionMock
                .Setup(x => x.UpdateOneAsync(
                    It.IsAny<FilterDefinition<User>>(),
                    It.IsAny<UpdateDefinition<User>>(),
                    It.IsAny<UpdateOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UpdateResult.Acknowledged(1, 1, null));

            // Act
            await _authService.DeactivateUserAsync(userId, currentUserId);

            // Assert
            _usersCollectionMock.Verify(x => x.UpdateOneAsync(It.IsAny<FilterDefinition<User>>(), It.IsAny<UpdateDefinition<User>>(), It.IsAny<UpdateOptions>(), It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task DeactivateUserAsync_NonAdminNonSelf_SuccessfullyDeactivatesUser()
        {
            // Arrange
            var userId = "67eacf021af224f20dca689f";
            var currentUserId = "different-user-id";
            var targetUser = new User
            {
                UserId = userId,
                Username = "lehoangphuc",
                Role = UserRole.STAFF,
                IsActive = true
            };

            var mockCursorTargetUser = new Mock<IAsyncCursor<User>>();
            mockCursorTargetUser.Setup(_ => _.Current).Returns(new List<User> { targetUser });
            mockCursorTargetUser
                .SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            _usersCollectionMock
                .Setup(x => x.FindAsync(
                    It.IsAny<FilterDefinition<User>>(),
                    It.IsAny<FindOptions<User, User>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursorTargetUser.Object);

            _usersCollectionMock
                .Setup(x => x.UpdateOneAsync(
                    It.IsAny<FilterDefinition<User>>(),
                    It.IsAny<UpdateDefinition<User>>(),
                    It.IsAny<UpdateOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UpdateResult.Acknowledged(1, 1, null));

            // Act
            await _authService.DeactivateUserAsync(userId, currentUserId);

            // Assert
            _usersCollectionMock.Verify(x => x.UpdateOneAsync(It.IsAny<FilterDefinition<User>>(), It.IsAny<UpdateDefinition<User>>(), It.IsAny<UpdateOptions>(), It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task DeactivateUserAsync_CurrentUserNotFound_SuccessfullyDeactivatesUser()
        {
            // Arrange
            var userId = "67eacf021af224f20dca689f";
            var currentUserId = "nonexistent";
            var targetUser = new User
            {
                UserId = userId,
                Username = "lehoangphuc",
                Role = UserRole.STAFF,
                IsActive = true
            };

            var mockCursorTargetUser = new Mock<IAsyncCursor<User>>();
            mockCursorTargetUser.Setup(_ => _.Current).Returns(new List<User> { targetUser });
            mockCursorTargetUser
                .SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            _usersCollectionMock
                .Setup(x => x.FindAsync(
                    It.IsAny<FilterDefinition<User>>(),
                    It.IsAny<FindOptions<User, User>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursorTargetUser.Object);

            _usersCollectionMock
                .Setup(x => x.UpdateOneAsync(
                    It.IsAny<FilterDefinition<User>>(),
                    It.IsAny<UpdateDefinition<User>>(),
                    It.IsAny<UpdateOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UpdateResult.Acknowledged(1, 1, null));

            // Act
            await _authService.DeactivateUserAsync(userId, currentUserId);

            // Assert
            _usersCollectionMock.Verify(x => x.UpdateOneAsync(It.IsAny<FilterDefinition<User>>(), It.IsAny<UpdateDefinition<User>>(), It.IsAny<UpdateOptions>(), It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task DeactivateUserAsync_TargetUserNotFound_ThrowsArgumentException()
        {
            // Arrange
            var userId = "non-existent-id";
            var currentUserId = "admin-id";

            var mockCursorTargetUser = new Mock<IAsyncCursor<User>>();
            mockCursorTargetUser.Setup(_ => _.Current).Returns(new List<User>());
            mockCursorTargetUser
                .SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _usersCollectionMock
                .Setup(x => x.FindAsync(
                    It.IsAny<FilterDefinition<User>>(),
                    It.IsAny<FindOptions<User, User>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursorTargetUser.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _authService.DeactivateUserAsync(userId, currentUserId));
            Assert.Equal("User không tồn tại.", exception.Message);
        }

        // Test GetAllUsersAsync
        [Fact]
        public async Task GetAllUsersAsync_ValidAdminUser_ReturnsUserList()
        {
            // Arrange
            var currentUserId = "admin-id";
            var users = new List<User>
            {
                new User
                {
                    UserId = "67eacf021af224f20dca689e",
                    Username = "tranthimai",
                    Email = "ttm@gmail.com",
                    PhoneNo = "09213972131",
                    Password = "nbejhkj12g",
                    Role = UserRole.STAFF,
                    IsActive = false,
                    FullName = "tanthimai"
                },
                new User
                {
                    UserId = "67eacf021af224f20dca689f",
                    Username = "lehoangphuc",
                    Email = "lehoangphuc@example.com",
                    PhoneNo = "09003345678",
                    Password = "hashed_password_3",
                    Role = UserRole.STAFF,
                    IsActive = false,
                    FullName = "Le Hoang Phuc"
                }
            };

            var mockCursorAllUsers = new Mock<IAsyncCursor<User>>();
            mockCursorAllUsers.Setup(_ => _.Current).Returns(users);
            mockCursorAllUsers
                .SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            _usersCollectionMock
                .Setup(x => x.FindAsync(
                    It.IsAny<FilterDefinition<User>>(),
                    It.IsAny<FindOptions<User, User>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursorAllUsers.Object);

            // Act
            var result = await _authService.GetAllUsersAsync(currentUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(users[0].UserId, result[0].UserId);
            Assert.Equal(users[1].UserId, result[1].UserId);
        }

        [Fact]
        public async Task GetAllUsersAsync_NonAdminUser_ReturnsUserList()
        {
            // Arrange
            var currentUserId = "67eacf021af224f20dca689f";
            var users = new List<User>
            {
                new User
                {
                    UserId = "67eacf021af224f20dca689e",
                    Username = "tranthimai",
                    Email = "ttm@gmail.com",
                    PhoneNo = "09213972131",
                    Password = "nbejhkj12g",
                    Role = UserRole.STAFF,
                    IsActive = false,
                    FullName = "tanthimai"
                },
                new User
                {
                    UserId = "67eacf021af224f20dca689f",
                    Username = "lehoangphuc",
                    Email = "lehoangphuc@example.com",
                    PhoneNo = "09003345678",
                    Password = "hashed_password_3",
                    Role = UserRole.STAFF,
                    IsActive = false,
                    FullName = "Le Hoang Phuc"
                }
            };

            var mockCursorAllUsers = new Mock<IAsyncCursor<User>>();
            mockCursorAllUsers.Setup(_ => _.Current).Returns(users);
            mockCursorAllUsers
                .SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            _usersCollectionMock
                .Setup(x => x.FindAsync(
                    It.IsAny<FilterDefinition<User>>(),
                    It.IsAny<FindOptions<User, User>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursorAllUsers.Object);

            // Act
            var result = await _authService.GetAllUsersAsync(currentUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(users[0].UserId, result[0].UserId);
            Assert.Equal(users[1].UserId, result[1].UserId);
        }

        [Fact]
        public async Task GetAllUsersAsync_CurrentUserNotFound_ReturnsUserList()
        {
            // Arrange
            var currentUserId = "nonexistent";
            var users = new List<User>
            {
                new User
                {
                    UserId = "67eacf021af224f20dca689e",
                    Username = "tranthimai",
                    Email = "ttm@gmail.com",
                    PhoneNo = "09213972131",
                    Password = "nbejhkj12g",
                    Role = UserRole.STAFF,
                    IsActive = false,
                    FullName = "tanthimai"
                },
                new User
                {
                    UserId = "67eacf021af224f20dca689f",
                    Username = "lehoangphuc",
                    Email = "lehoangphuc@example.com",
                    PhoneNo = "09003345678",
                    Password = "hashed_password_3",
                    Role = UserRole.STAFF,
                    IsActive = false,
                    FullName = "Le Hoang Phuc"
                }
            };

            var mockCursorAllUsers = new Mock<IAsyncCursor<User>>();
            mockCursorAllUsers.Setup(_ => _.Current).Returns(users);
            mockCursorAllUsers
                .SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            _usersCollectionMock
                .Setup(x => x.FindAsync(
                    It.IsAny<FilterDefinition<User>>(),
                    It.IsAny<FindOptions<User, User>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursorAllUsers.Object);

            // Act
            var result = await _authService.GetAllUsersAsync(currentUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(users[0].UserId, result[0].UserId);
            Assert.Equal(users[1].UserId, result[1].UserId);
        }
    }
}