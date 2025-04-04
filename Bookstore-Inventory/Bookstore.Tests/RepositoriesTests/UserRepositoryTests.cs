using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bookstore.Domain.Entities;
using Bookstore.Infrastructure.Data;
using Bookstore.Infrastructure.Repositories;
using MongoDB.Driver;
using Moq;
using Xunit;

namespace Bookstore.Tests.RepositoriesTests
{
    public class UserRepositoryTests
    {
        private readonly Mock<IMongoCollection<User>> _mockCollection;
        private readonly Mock<MongoDbContext> _mockDbContext;
        private readonly UserRepository _userRepository;

        public UserRepositoryTests()
        {
            _mockCollection = new Mock<IMongoCollection<User>>();
            _mockDbContext = new Mock<MongoDbContext>();
            _mockDbContext.Setup(x => x.GetCollection<User>("User")).Returns(_mockCollection.Object);
            _userRepository = new UserRepository(_mockDbContext.Object);

            // Diagnostic logging to verify setup
            Console.WriteLine("Test setup completed.");
            Console.WriteLine($"_mockCollection is null: {_mockCollection == null}");
            Console.WriteLine($"_mockDbContext is null: {_mockDbContext == null}");
            Console.WriteLine($"_userRepository is null: {_userRepository == null}");
        }

        [Fact]
        public async Task GetAllUsersAsync_ReturnsListOfUsers()
        {
            // Arrange
            var users = new List<User>
            {
                new User
                {
                    UserId = "67eacf021af224f20dca689e",
                    Username = "tranthimai",
                    Email = "ttm@gmail.com",
                    PhoneNo = "09213972131",
                    Password = "hashed_password_1",
                    Role = UserRole.STAFF,
                    IsActive = false,
                    FullName = "tranthimai"
                },
                new User
                {
                    UserId = "67eacf021af224f20dca689d",
                    Username = "huynhnhungoc",
                    Email = "huynhnhungoc@example.com",
                    PhoneNo = "0901123456",
                    Password = "hashed_password_1",
                    Role = UserRole.ADMINISTRATOR,
                    IsActive = true,
                    FullName = "Huynh Nhu Ngoc"
                }
            };

            _mockCollection.Setup(x => x.FindAsync(
                It.IsAny<FilterDefinition<User>>(),
                It.IsAny<FindOptions<User, User>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(users.AsAsyncCursor());

            // Act
            Console.WriteLine("Calling GetAllUsersAsync...");
            var result = await _userRepository.GetAllUsersAsync();
            Console.WriteLine($"GetAllUsersAsync returned {result?.Count ?? 0} users.");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains(result, u => u.Username == "tranthimai" && u.Role == UserRole.STAFF && !u.IsActive);
            Assert.Contains(result, u => u.Username == "huynhnhungoc" && u.Role == UserRole.ADMINISTRATOR && u.IsActive);
        }

        [Fact]
        public async Task GetUserByIdAsync_ReturnsUser_WhenUserExists()
        {
            // Arrange
            var user = new User
            {
                UserId = "67eacf021af224f20dca689d",
                Username = "huynhnhungoc",
                Email = "huynhnhungoc@example.com",
                PhoneNo = "0901123456",
                Password = "hashed_password_1",
                Role = UserRole.ADMINISTRATOR,
                IsActive = true,
                FullName = "Huynh Nhu Ngoc"
            };

            _mockCollection.Setup(x => x.FindAsync(
                It.IsAny<FilterDefinition<User>>(),
                It.IsAny<FindOptions<User, User>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<User> { user }.AsAsyncCursor());

            // Act
            Console.WriteLine("Calling GetUserByIdAsync...");
            var result = await _userRepository.GetUserByIdAsync("67eacf021af224f20dca689d");
            Console.WriteLine($"GetUserByIdAsync returned user: {result?.Username ?? "null"}");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("67eacf021af224f20dca689d", result.UserId);
            Assert.Equal("huynhnhungoc", result.Username);
        }

        [Fact]
        public async Task GetUserByIdAsync_ReturnsNull_WhenUserDoesNotExist()
        {
            // Arrange
            _mockCollection.Setup(x => x.FindAsync(
                It.IsAny<FilterDefinition<User>>(),
                It.IsAny<FindOptions<User, User>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<User>().AsAsyncCursor());

            // Act
            Console.WriteLine("Calling GetUserByIdAsync for non-existent user...");
            var result = await _userRepository.GetUserByIdAsync("999");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AddUserAsync_InsertsUser()
        {
            // Arrange
            var user = new User
            {
                UserId = "67eacf021af224f20dca689e",
                Username = "tranthimai",
                Email = "ttm@gmail.com",
                PhoneNo = "09213972131",
                Password = "hashed_password_1",
                Role = UserRole.STAFF,
                IsActive = false,
                FullName = "tranthimai"
            };

            _mockCollection.Setup(x => x.InsertOneAsync(
                It.IsAny<User>(),
                It.IsAny<InsertOneOptions>(),
                It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            Console.WriteLine("Calling AddUserAsync...");
            await _userRepository.AddUserAsync(user);
            Console.WriteLine("AddUserAsync completed.");

            // Assert
            _mockCollection.Verify(x => x.InsertOneAsync(
                It.Is<User>(u => u.UserId == "67eacf021af224f20dca689e"),
                It.IsAny<InsertOneOptions>(),
                It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task UpdateUserAsync_ReplacesUser()
        {
            // Arrange
            var user = new User
            {
                UserId = "67eacf021af224f20dca689d",
                Username = "huynhnhungoc",
                Email = "huynhnhungoc@example.com",
                PhoneNo = "0901123456",
                Password = "hashed_password_1",
                Role = UserRole.ADMINISTRATOR,
                IsActive = true,
                FullName = "Huynh Nhu Ngoc"
            };

            _mockCollection.Setup(x => x.ReplaceOneAsync(
                It.IsAny<FilterDefinition<User>>(),
                It.IsAny<User>(),
                It.IsAny<ReplaceOptions>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ReplaceOneResult.Acknowledged(1, 1, null));

            // Act
            Console.WriteLine("Calling UpdateUserAsync...");
            await _userRepository.UpdateUserAsync(user);
            Console.WriteLine("UpdateUserAsync completed.");

            // Assert
            _mockCollection.Verify(x => x.ReplaceOneAsync(
                It.IsAny<FilterDefinition<User>>(),
                It.Is<User>(u => u.UserId == "67eacf021af224f20dca689d"),
                It.IsAny<ReplaceOptions>(),
                It.IsAny<CancellationToken>()), Times.Once());
        }
    }

    // Helper extension to simplify IAsyncCursor creation
    public static class AsyncCursorExtensions
    {
        public static IAsyncCursor<T> AsAsyncCursor<T>(this IEnumerable<T> source)
        {
            var mockCursor = new Mock<IAsyncCursor<T>>();
            mockCursor.Setup(x => x.Current).Returns(source);
            mockCursor.SetupSequence(x => x.MoveNext(It.IsAny<CancellationToken>())).Returns(true).Returns(false);
            mockCursor.SetupSequence(x => x.MoveNextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true).ReturnsAsync(false);
            return mockCursor.Object;
        }
    }
}