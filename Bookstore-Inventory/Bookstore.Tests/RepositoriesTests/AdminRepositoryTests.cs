using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bookstore.Domain.Entities;
using Bookstore.Infrastructure.Repositories;
using MongoDB.Driver;
using Moq;
using Xunit;

namespace Bookstore.Tests.RepositoriesTests
{
    public class AdminRepositoryTests
    {
        private readonly Mock<IMongoCollection<Admin>> _mockCollection;
        private readonly Mock<IMongoDatabase> _mockDatabase;
        private readonly AdminRepository _adminRepository;

        public AdminRepositoryTests()
        {
            _mockCollection = new Mock<IMongoCollection<Admin>>();
            _mockDatabase = new Mock<IMongoDatabase>();
            _mockDatabase.Setup(x => x.GetCollection<Admin>("Admin", null)).Returns(_mockCollection.Object);
            _adminRepository = new AdminRepository(_mockDatabase.Object);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsListOfAdmins()
        {
            // Arrange
            var adminList = new List<Admin>
            {
                new Admin { AdminId = "67eaec3b1af224f20dca6987f", UserId = "67eac021af224f20dca689d" },
                new Admin { AdminId = "67eaec3b1af224f20dca6981", UserId = "67eac021af224f20dca68a3" },
                new Admin { AdminId = "67eaec3b1af224f20dca6980", UserId = "67eac021af224f20dca68a0" },
                new Admin { AdminId = "67eaec3b1af224f20dca6982", UserId = "67eac021af224f20dca68a6" }
            };
            var mockCursor = new Mock<IAsyncCursor<Admin>>();
            mockCursor.Setup(x => x.Current).Returns(adminList);
            mockCursor.SetupSequence(x => x.MoveNextAsync(CancellationToken.None))
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            _mockCollection.Setup(x => x.FindAsync(
                It.IsAny<FilterDefinition<Admin>>(),
                It.IsAny<FindOptions<Admin, Admin>>(),
                CancellationToken.None))
                .ReturnsAsync(mockCursor.Object);

            // Act
            var result = await _adminRepository.GetAllAsync();

            // Assert
            Assert.Equal(4, result.Count());
            Assert.Equal("67eaec3b1af224f20dca6987f", result.ElementAt(0).AdminId);
            Assert.Equal("67eaec3b1af224f20dca6982", result.ElementAt(3).AdminId);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsAdmin_WhenAdminExists()
        {
            // Arrange
            var admin = new Admin { AdminId = "67eaec3b1af224f20dca6987f", UserId = "67eac021af224f20dca689d" };
            var mockCursor = new Mock<IAsyncCursor<Admin>>();
            mockCursor.Setup(x => x.Current).Returns(new List<Admin> { admin });
            mockCursor.SetupSequence(x => x.MoveNextAsync(CancellationToken.None))
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            _mockCollection.Setup(x => x.FindAsync(
                It.IsAny<FilterDefinition<Admin>>(),
                It.IsAny<FindOptions<Admin, Admin>>(),
                CancellationToken.None))
                .ReturnsAsync(mockCursor.Object);

            // Act
            var result = await _adminRepository.GetByIdAsync("67eaec3b1af224f20dca6987f");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("67eaec3b1af224f20dca6987f", result.AdminId);
            Assert.Equal("67eac021af224f20dca689d", result.UserId);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenAdminDoesNotExist()
        {
            // Arrange
            var mockCursor = new Mock<IAsyncCursor<Admin>>();
            mockCursor.Setup(x => x.Current).Returns(new List<Admin>());
            mockCursor.SetupSequence(x => x.MoveNextAsync(CancellationToken.None))
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            _mockCollection.Setup(x => x.FindAsync(
                It.IsAny<FilterDefinition<Admin>>(),
                It.IsAny<FindOptions<Admin, Admin>>(),
                CancellationToken.None))
                .ReturnsAsync(mockCursor.Object);

            // Act
            var result = await _adminRepository.GetByIdAsync("999");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AddAsync_InsertsAdmin()
        {
            // Arrange
            var admin = new Admin { AdminId = "67eaec3b1af224f20dca6987f", UserId = "67eac021af224f20dca689d" };

            // Act
            await _adminRepository.AddAsync(admin);

            // Assert
            _mockCollection.Verify(x => x.InsertOneAsync(admin, null, CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ReplacesAdmin()
        {
            // Arrange
            var admin = new Admin { AdminId = "67eaec3b1af224f20dca6987f", UserId = "67eac021af224f20dca689d" };

            // Act
            await _adminRepository.UpdateAsync(admin);

            // Assert
            _mockCollection.Verify(x => x.ReplaceOneAsync(
                It.IsAny<FilterDefinition<Admin>>(),
                admin,
                It.IsAny<ReplaceOptions>(),
                CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_DeletesAdmin()
        {
            // Arrange
            var adminId = "67eaec3b1af224f20dca6987f";

            // Act
            await _adminRepository.DeleteAsync(adminId);

            // Assert
            _mockCollection.Verify(x => x.DeleteOneAsync(
                It.IsAny<FilterDefinition<Admin>>(),
                CancellationToken.None), Times.Once);
        }
    }
}