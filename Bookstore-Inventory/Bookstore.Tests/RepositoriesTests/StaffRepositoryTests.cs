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
    public class StaffRepositoryTests
    {
        private readonly Mock<IMongoCollection<Staff>> _mockCollection;
        private readonly Mock<IMongoDatabase> _mockDatabase;
        private readonly StaffRepository _staffRepository;

        public StaffRepositoryTests()
        {
            _mockCollection = new Mock<IMongoCollection<Staff>>();
            _mockDatabase = new Mock<IMongoDatabase>();
            _mockDatabase.Setup(x => x.GetCollection<Staff>("Staff", null)).Returns(_mockCollection.Object);
            _staffRepository = new StaffRepository(_mockDatabase.Object);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsListOfStaff()
        {
            // Arrange
            var staffList = new List<Staff>
            {
                new Staff { StaffId = "67eaec3b1af224f20dca6985", UserId = "67eac021af224f20dca689f" }
            };
            var mockCursor = new Mock<IAsyncCursor<Staff>>();
            mockCursor.Setup(x => x.Current).Returns(staffList);
            mockCursor.SetupSequence(x => x.MoveNextAsync(CancellationToken.None))
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            _mockCollection.Setup(x => x.FindAsync(
                It.IsAny<FilterDefinition<Staff>>(),
                It.IsAny<FindOptions<Staff, Staff>>(),
                CancellationToken.None))
                .ReturnsAsync(mockCursor.Object);

            // Act
            var result = await _staffRepository.GetAllAsync();

            // Assert
            Assert.Single(result);
            Assert.Equal("67eaec3b1af224f20dca6985", result.ElementAt(0).StaffId);
            Assert.Equal("67eac021af224f20dca689f", result.ElementAt(0).UserId);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsStaff_WhenStaffExists()
        {
            // Arrange
            var staff = new Staff { StaffId = "67eaec3b1af224f20dca6985", UserId = "67eac021af224f20dca689f" };
            var mockCursor = new Mock<IAsyncCursor<Staff>>();
            mockCursor.Setup(x => x.Current).Returns(new List<Staff> { staff });
            mockCursor.SetupSequence(x => x.MoveNextAsync(CancellationToken.None))
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            _mockCollection.Setup(x => x.FindAsync(
                It.IsAny<FilterDefinition<Staff>>(),
                It.IsAny<FindOptions<Staff, Staff>>(),
                CancellationToken.None))
                .ReturnsAsync(mockCursor.Object);

            // Act
            var result = await _staffRepository.GetByIdAsync("67eaec3b1af224f20dca6985");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("67eac021af224f20dca689f", result.UserId);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenStaffDoesNotExist()
        {
            // Arrange
            var mockCursor = new Mock<IAsyncCursor<Staff>>();
            mockCursor.Setup(x => x.Current).Returns(new List<Staff>());
            mockCursor.SetupSequence(x => x.MoveNextAsync(CancellationToken.None))
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            _mockCollection.Setup(x => x.FindAsync(
                It.IsAny<FilterDefinition<Staff>>(),
                It.IsAny<FindOptions<Staff, Staff>>(),
                CancellationToken.None))
                .ReturnsAsync(mockCursor.Object);

            // Act
            var result = await _staffRepository.GetByIdAsync("999");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AddAsync_InsertsStaff()
        {
            // Arrange
            var staff = new Staff { StaffId = "67eaec3b1af224f20dca6985", UserId = "67eac021af224f20dca689f" };

            // Act
            await _staffRepository.AddAsync(staff);

            // Assert
            _mockCollection.Verify(x => x.InsertOneAsync(staff, null, CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ReplacesStaff()
        {
            // Arrange
            var staff = new Staff { StaffId = "67eaec3b1af224f20dca6985", UserId = "67eac021af224f20dca689f" };

            // Act
            await _staffRepository.UpdateAsync(staff);

            // Assert
            _mockCollection.Verify(x => x.ReplaceOneAsync(
                It.IsAny<FilterDefinition<Staff>>(),
                staff,
                It.IsAny<ReplaceOptions>(),
                CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_DeletesStaff()
        {
            // Arrange
            var staffId = "67eaec3b1af224f20dca6985";

            // Act
            await _staffRepository.DeleteAsync(staffId);

            // Assert
            _mockCollection.Verify(x => x.DeleteOneAsync(
                It.IsAny<FilterDefinition<Staff>>(),
                CancellationToken.None), Times.Once);
        }
    }
}