using Bookstore.Infrastructure.Repositories;
using Bookstore.Domain.Entities;
using MongoDB.Driver;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Bookstore.Infrastructure.Tests.Repositories
{
    public class WarehouseImportRepositoryTests
    {
        private readonly Mock<IMongoCollection<WarehouseImport>> _mockCollection;
        private readonly Mock<IMongoDatabase> _mockDatabase;
        private readonly WarehouseImportRepository _repository;

        public WarehouseImportRepositoryTests()
        {
            // Khởi tạo mock cho IMongoCollection
            _mockCollection = new Mock<IMongoCollection<WarehouseImport>>();

            // Khởi tạo mock cho IMongoDatabase
            _mockDatabase = new Mock<IMongoDatabase>();
            _mockDatabase
                .Setup(db => db.GetCollection<WarehouseImport>("WarehouseImport", It.IsAny<MongoCollectionSettings>()))
                .Returns(_mockCollection.Object);

            // Khởi tạo repository
            _repository = new WarehouseImportRepository(_mockDatabase.Object);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllWarehouseImports()
        {
            // Arrange
            var warehouseImports = new List<WarehouseImport>
            {
                new WarehouseImport
                {
                    ImportId = "67eae6131af224f20dca6931",
                    ImportDate = DateTime.Parse("2025-04-01T00:00:00.002Z"),
                    UserId = "67eacf021af224f20dca689d",
                    WarehouseImportBooks = new List<WarehouseImportBook>
                    {
                        new WarehouseImportBook
                        {
                            BookId = "67eadd6a1af224f20dca68cd",
                            ImportQuantity = 15,
                            Price = 3500000
                        }
                    }
                },
                new WarehouseImport
                {
                    ImportId = "67eae6131af224f20dca6935",
                    ImportDate = DateTime.Parse("2025-04-01T00:00:00.002Z"),
                    UserId = "67eacf021af224f20dca689e",
                    WarehouseImportBooks = new List<WarehouseImportBook>
                    {
                        new WarehouseImportBook
                        {
                            BookId = "67eadd6a1af224f20dca68ce",
                            ImportQuantity = 12,
                            Price = 3700000
                        }
                    }
                }
            };

            var mockCursor = new Mock<IAsyncCursor<WarehouseImport>>();
            mockCursor.Setup(_ => _.Current).Returns(warehouseImports);
            mockCursor.SetupSequence(_ => _.MoveNext(It.IsAny<CancellationToken>())).Returns(true).Returns(false);
            mockCursor.SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true).ReturnsAsync(false);

            _mockCollection
                .Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<WarehouseImport>>(),
                    It.IsAny<FindOptions<WarehouseImport, WarehouseImport>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            var firstImport = result.First();
            Assert.Equal("67eae6131af224f20dca6931", firstImport.ImportId);
            Assert.Equal("67eacf021af224f20dca689d", firstImport.UserId);
            Assert.Equal(1, firstImport.WarehouseImportBooks.Count);
            Assert.Equal("67eadd6a1af224f20dca68cd", firstImport.WarehouseImportBooks[0].BookId);
            Assert.Equal(15, firstImport.WarehouseImportBooks[0].ImportQuantity);
            Assert.Equal(3500000, firstImport.WarehouseImportBooks[0].Price);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnWarehouseImport_WhenIdExists()
        {
            // Arrange
            var importId = "67eae6131af224f20dca6931";
            var warehouseImport = new WarehouseImport
            {
                ImportId = importId,
                ImportDate = DateTime.Parse("2025-04-01T00:00:00.002Z"),
                UserId = "67eacf021af224f20dca689d",
                WarehouseImportBooks = new List<WarehouseImportBook>
                {
                    new WarehouseImportBook
                    {
                        BookId = "67eadd6a1af224f20dca68cd",
                        ImportQuantity = 15,
                        Price = 3500000
                    }
                }
            };

            var mockCursor = new Mock<IAsyncCursor<WarehouseImport>>();
            mockCursor.Setup(_ => _.Current).Returns(new List<WarehouseImport> { warehouseImport });
            mockCursor.SetupSequence(_ => _.MoveNext(It.IsAny<CancellationToken>())).Returns(true).Returns(false);
            mockCursor.SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true).ReturnsAsync(false);

            _mockCollection
                .Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<WarehouseImport>>(),
                    It.IsAny<FindOptions<WarehouseImport, WarehouseImport>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            // Act
            var result = await _repository.GetByIdAsync(importId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(importId, result.ImportId);
            Assert.Equal("67eacf021af224f20dca689d", result.UserId);
            Assert.Equal(1, result.WarehouseImportBooks.Count);
            Assert.Equal("67eadd6a1af224f20dca68cd", result.WarehouseImportBooks[0].BookId);
            Assert.Equal(15, result.WarehouseImportBooks[0].ImportQuantity);
            Assert.Equal(3500000, result.WarehouseImportBooks[0].Price);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenIdDoesNotExist()
        {
            // Arrange
            var importId = "non-existent-id";

            var mockCursor = new Mock<IAsyncCursor<WarehouseImport>>();
            mockCursor.Setup(_ => _.Current).Returns(new List<WarehouseImport>());
            mockCursor.SetupSequence(_ => _.MoveNext(It.IsAny<CancellationToken>())).Returns(false);
            mockCursor.SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false);

            _mockCollection
                .Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<WarehouseImport>>(),
                    It.IsAny<FindOptions<WarehouseImport, WarehouseImport>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            // Act
            var result = await _repository.GetByIdAsync(importId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AddAsync_ShouldInsertWarehouseImport()
        {
            // Arrange
            var warehouseImport = new WarehouseImport
            {
                ImportId = "67eae6131af224f20dca6931",
                ImportDate = DateTime.Parse("2025-04-01T00:00:00.002Z"),
                UserId = "67eacf021af224f20dca689d",
                WarehouseImportBooks = new List<WarehouseImportBook>()
            };

            _mockCollection
                .Setup(c => c.InsertOneAsync(
                    It.IsAny<WarehouseImport>(),
                    It.IsAny<InsertOneOptions>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _repository.AddAsync(warehouseImport);

            // Assert
            _mockCollection.Verify(c => c.InsertOneAsync(
                It.Is<WarehouseImport>(wi => wi.ImportId == warehouseImport.ImportId),
                It.IsAny<InsertOneOptions>(),
                It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task UpdateAsync_ShouldReplaceWarehouseImport()
        {
            // Arrange
            var warehouseImport = new WarehouseImport
            {
                ImportId = "67eae6131af224f20dca6931",
                ImportDate = DateTime.Parse("2025-04-01T00:00:00.002Z"),
                UserId = "67eacf021af224f20dca689d",
                WarehouseImportBooks = new List<WarehouseImportBook>()
            };

            _mockCollection
                .Setup(c => c.ReplaceOneAsync(
                    It.IsAny<FilterDefinition<WarehouseImport>>(),
                    It.IsAny<WarehouseImport>(),
                    It.IsAny<ReplaceOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ReplaceOneResult.Acknowledged(1, 1, null));

            // Act
            await _repository.UpdateAsync(warehouseImport);

            // Assert
            _mockCollection.Verify(c => c.ReplaceOneAsync(
                It.IsAny<FilterDefinition<WarehouseImport>>(),
                It.Is<WarehouseImport>(wi => wi.ImportId == warehouseImport.ImportId),
                It.IsAny<ReplaceOptions>(),
                It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveWarehouseImport()
        {
            // Arrange
            var importId = "67eae6131af224f20dca6931";

            _mockCollection
                .Setup(c => c.DeleteOneAsync(
                    It.IsAny<FilterDefinition<WarehouseImport>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DeleteResult.Acknowledged(1));

            // Act
            await _repository.DeleteAsync(importId);

            // Assert
            _mockCollection.Verify(c => c.DeleteOneAsync(
                It.IsAny<FilterDefinition<WarehouseImport>>(),
                It.IsAny<CancellationToken>()), Times.Once());
        }
    }
}