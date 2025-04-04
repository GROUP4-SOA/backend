using Bookstore.Infrastructure.Data;
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
    public class WarehouseExportRepositoryTests
    {
        private readonly Mock<IMongoCollection<WarehouseExport>> _mockCollection;
        private readonly Mock<MongoDbContext> _mockDbContext;
        private readonly WarehouseExportRepository _repository;

        public WarehouseExportRepositoryTests()
        {
            // Khởi tạo mock cho IMongoCollection
            _mockCollection = new Mock<IMongoCollection<WarehouseExport>>();

            // Khởi tạo mock cho MongoDbContext
            _mockDbContext = new Mock<MongoDbContext>();
            _mockDbContext
                .Setup(db => db.GetCollection<WarehouseExport>("WarehouseExport"))
                .Returns(_mockCollection.Object);

            // Khởi tạo repository
            _repository = new WarehouseExportRepository(_mockDbContext.Object);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllWarehouseExports()
        {
            // Arrange
            var warehouseExports = new List<WarehouseExport>
            {
                new WarehouseExport
                {
                    ExportId = "67eae55c1af224f20dca6929",
                    ExportDate = DateTime.Parse("2025-04-02T00:00:00.002Z"),
                    UserId = "67eacf021af224f20dca689e",
                    WarehouseExportBooks = new List<WarehouseExportBook>
                    {
                        new WarehouseExportBook
                        {
                            BookId = "67eadd6a1af224f20dca6805",
                            ExportQuantity = 7,
                            Price = 4900000
                        },
                        new WarehouseExportBook
                        {
                            BookId = "67eadd6a1af224f20dca6809",
                            ExportQuantity = 2,
                            Price = 4500000
                        }
                    }
                },
                new WarehouseExport
                {
                    ExportId = "67eae55c1af224f20dca6923",
                    ExportDate = DateTime.Parse("2025-04-01T00:00:00.002Z"),
                    UserId = "67eacf021af224f20dca689d",
                    WarehouseExportBooks = new List<WarehouseExportBook>()
                }
            };

            var mockCursor = new Mock<IAsyncCursor<WarehouseExport>>();
            mockCursor.Setup(_ => _.Current).Returns(warehouseExports);
            mockCursor.SetupSequence(_ => _.MoveNext(It.IsAny<CancellationToken>())).Returns(true).Returns(false);
            mockCursor.SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true).ReturnsAsync(false);

            _mockCollection
                .Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<WarehouseExport>>(),
                    It.IsAny<FindOptions<WarehouseExport, WarehouseExport>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            var firstExport = result.First();
            Assert.Equal("67eae55c1af224f20dca6929", firstExport.ExportId);
            Assert.Equal("67eacf021af224f20dca689e", firstExport.UserId);
            Assert.Equal(2, firstExport.WarehouseExportBooks.Count);
            Assert.Equal("67eadd6a1af224f20dca6805", firstExport.WarehouseExportBooks[0].BookId);
            Assert.Equal(7, firstExport.WarehouseExportBooks[0].ExportQuantity);
            Assert.Equal(4900000, firstExport.WarehouseExportBooks[0].Price);
            Assert.Equal("67eadd6a1af224f20dca6809", firstExport.WarehouseExportBooks[1].BookId);
            Assert.Equal(2, firstExport.WarehouseExportBooks[1].ExportQuantity);
            Assert.Equal(4500000, firstExport.WarehouseExportBooks[1].Price);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnWarehouseExport_WhenIdExists()
        {
            // Arrange
            var exportId = "67eae55c1af224f20dca6929";
            var warehouseExport = new WarehouseExport
            {
                ExportId = exportId,
                ExportDate = DateTime.Parse("2025-04-02T00:00:00.002Z"),
                UserId = "67eacf021af224f20dca689e",
                WarehouseExportBooks = new List<WarehouseExportBook>
                {
                    new WarehouseExportBook
                    {
                        BookId = "67eadd6a1af224f20dca6805",
                        ExportQuantity = 7,
                        Price = 4900000
                    },
                    new WarehouseExportBook
                    {
                        BookId = "67eadd6a1af224f20dca6809",
                        ExportQuantity = 2,
                        Price = 4500000
                    }
                }
            };

            var mockCursor = new Mock<IAsyncCursor<WarehouseExport>>();
            mockCursor.Setup(_ => _.Current).Returns(new List<WarehouseExport> { warehouseExport });
            mockCursor.SetupSequence(_ => _.MoveNext(It.IsAny<CancellationToken>())).Returns(true).Returns(false);
            mockCursor.SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true).ReturnsAsync(false);

            _mockCollection
                .Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<WarehouseExport>>(),
                    It.IsAny<FindOptions<WarehouseExport, WarehouseExport>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            // Act
            var result = await _repository.GetByIdAsync(exportId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(exportId, result.ExportId);
            Assert.Equal("67eacf021af224f20dca689e", result.UserId);
            Assert.Equal(2, result.WarehouseExportBooks.Count);
            Assert.Equal("67eadd6a1af224f20dca6805", result.WarehouseExportBooks[0].BookId);
            Assert.Equal(7, result.WarehouseExportBooks[0].ExportQuantity);
            Assert.Equal(4900000, result.WarehouseExportBooks[0].Price);
            Assert.Equal("67eadd6a1af224f20dca6809", result.WarehouseExportBooks[1].BookId);
            Assert.Equal(2, result.WarehouseExportBooks[1].ExportQuantity);
            Assert.Equal(4500000, result.WarehouseExportBooks[1].Price);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenIdDoesNotExist()
        {
            // Arrange
            var exportId = "non-existent-id";

            var mockCursor = new Mock<IAsyncCursor<WarehouseExport>>();
            mockCursor.Setup(_ => _.Current).Returns(new List<WarehouseExport>());
            mockCursor.SetupSequence(_ => _.MoveNext(It.IsAny<CancellationToken>())).Returns(false);
            mockCursor.SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false);

            _mockCollection
                .Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<WarehouseExport>>(),
                    It.IsAny<FindOptions<WarehouseExport, WarehouseExport>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            // Act
            var result = await _repository.GetByIdAsync(exportId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AddAsync_ShouldInsertWarehouseExport()
        {
            // Arrange
            var warehouseExport = new WarehouseExport
            {
                ExportId = "67eae55c1af224f20dca6929",
                ExportDate = DateTime.Parse("2025-04-02T00:00:00.002Z"),
                UserId = "67eacf021af224f20dca689e",
                WarehouseExportBooks = new List<WarehouseExportBook>()
            };

            _mockCollection
                .Setup(c => c.InsertOneAsync(
                    It.IsAny<WarehouseExport>(),
                    It.IsAny<InsertOneOptions>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _repository.AddAsync(warehouseExport);

            // Assert
            _mockCollection.Verify(c => c.InsertOneAsync(
                It.Is<WarehouseExport>(we => we.ExportId == warehouseExport.ExportId),
                It.IsAny<InsertOneOptions>(),
                It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task UpdateAsync_ShouldReplaceWarehouseExport()
        {
            // Arrange
            var warehouseExport = new WarehouseExport
            {
                ExportId = "67eae55c1af224f20dca6929",
                ExportDate = DateTime.Parse("2025-04-02T00:00:00.002Z"),
                UserId = "67eacf021af224f20dca689e",
                WarehouseExportBooks = new List<WarehouseExportBook>()
            };

            _mockCollection
                .Setup(c => c.ReplaceOneAsync(
                    It.IsAny<FilterDefinition<WarehouseExport>>(),
                    It.IsAny<WarehouseExport>(),
                    It.IsAny<ReplaceOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ReplaceOneResult.Acknowledged(1, 1, null));

            // Act
            await _repository.UpdateAsync(warehouseExport);

            // Assert
            _mockCollection.Verify(c => c.ReplaceOneAsync(
                It.IsAny<FilterDefinition<WarehouseExport>>(),
                It.Is<WarehouseExport>(we => we.ExportId == warehouseExport.ExportId),
                It.IsAny<ReplaceOptions>(),
                It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveWarehouseExport()
        {
            // Arrange
            var exportId = "67eae55c1af224f20dca6929";

            _mockCollection
                .Setup(c => c.DeleteOneAsync(
                    It.IsAny<FilterDefinition<WarehouseExport>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DeleteResult.Acknowledged(1));

            // Act
            await _repository.DeleteAsync(exportId);

            // Assert
            _mockCollection.Verify(c => c.DeleteOneAsync(
                It.IsAny<FilterDefinition<WarehouseExport>>(),
                It.IsAny<CancellationToken>()), Times.Once());
        }
    }
}