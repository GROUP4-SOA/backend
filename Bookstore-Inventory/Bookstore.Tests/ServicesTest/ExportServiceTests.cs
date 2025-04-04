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
    public class ExportServiceTests
    {
        private readonly Mock<IMongoCollection<WarehouseExport>> _exportsCollectionMock;
        private readonly Mock<IMongoCollection<WarehouseExportBook>> _exportBooksCollectionMock;
        private readonly Mock<IMongoCollection<Book>> _booksCollectionMock;
        private readonly Mock<IMongoCollection<User>> _usersCollectionMock;
        private readonly ExportService _exportService;

        public ExportServiceTests()
        {
            var databaseMock = new Mock<IMongoDatabase>();
            _exportsCollectionMock = new Mock<IMongoCollection<WarehouseExport>>();
            _exportBooksCollectionMock = new Mock<IMongoCollection<WarehouseExportBook>>();
            _booksCollectionMock = new Mock<IMongoCollection<Book>>();
            _usersCollectionMock = new Mock<IMongoCollection<User>>();

            databaseMock.Setup(db => db.GetCollection<WarehouseExport>("WarehouseExport", null)).Returns(_exportsCollectionMock.Object);
            databaseMock.Setup(db => db.GetCollection<WarehouseExportBook>("WarehouseExportBook", null)).Returns(_exportBooksCollectionMock.Object);
            databaseMock.Setup(db => db.GetCollection<Book>("Book", null)).Returns(_booksCollectionMock.Object);
            databaseMock.Setup(db => db.GetCollection<User>("User", null)).Returns(_usersCollectionMock.Object);

            _exportService = new ExportService(databaseMock.Object);
        }

        [Fact]
        public async Task CreateExportAsync_ValidExport_CreatesExportAndUpdatesBooks()
        {
            // Arrange
            var exportCreateDto = new WarehouseExportDto
            {
                ExportId = "67eae55c1af224f20dca6929", // From database sample
                UserId = "67eacf021af224f20dca689e", // From database sample
                ExportDate = DateTime.Parse("2025-04-02T00:00:00.002Z"),
                WarehouseExportBooks = new List<WarehouseExportBookDto>
                {
                    new WarehouseExportBookDto { BookId = "67eadd6a1af224f20dca6805", ExportQuantity = 7, Price = 4900000 }, // From database sample
                    new WarehouseExportBookDto { BookId = "67eadd6a1af224f20dca6809", ExportQuantity = 2, Price = 4500000 } // From database sample
                }
            };

            var user = new User { UserId = "67eacf021af224f20dca689e", Role = UserRole.STAFF, IsActive = true };
            var book1 = new Book { BookId = "67eadd6a1af224f20dca6805", Quantity = 20 }; // Enough quantity
            var book2 = new Book { BookId = "67eadd6a1af224f20dca6809", Quantity = 15 }; // Enough quantity

            var userCursorMock = new Mock<IAsyncCursor<User>>();
            userCursorMock.Setup(x => x.Current).Returns(new List<User> { user });
            userCursorMock.SetupSequence(x => x.MoveNext(It.IsAny<CancellationToken>())).Returns(true).Returns(false);
            userCursorMock.SetupSequence(x => x.MoveNextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true).ReturnsAsync(false);

            var bookCursorMock1 = new Mock<IAsyncCursor<Book>>();
            bookCursorMock1.Setup(x => x.Current).Returns(new List<Book> { book1 });
            bookCursorMock1.SetupSequence(x => x.MoveNext(It.IsAny<CancellationToken>())).Returns(true).Returns(false);
            bookCursorMock1.SetupSequence(x => x.MoveNextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true).ReturnsAsync(false);

            var bookCursorMock2 = new Mock<IAsyncCursor<Book>>();
            bookCursorMock2.Setup(x => x.Current).Returns(new List<Book> { book2 });
            bookCursorMock2.SetupSequence(x => x.MoveNext(It.IsAny<CancellationToken>())).Returns(true).Returns(false);
            bookCursorMock2.SetupSequence(x => x.MoveNextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true).ReturnsAsync(false);

            _usersCollectionMock
                .Setup(x => x.FindAsync(
                    It.IsAny<FilterDefinition<User>>(),
                    It.IsAny<FindOptions<User, User>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(userCursorMock.Object);

            _booksCollectionMock
                .SetupSequence(x => x.FindAsync(
                    It.IsAny<FilterDefinition<Book>>(),
                    It.IsAny<FindOptions<Book, Book>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(bookCursorMock1.Object)
                .ReturnsAsync(bookCursorMock2.Object);

            _booksCollectionMock
                .Setup(x => x.UpdateOneAsync(
                    It.IsAny<FilterDefinition<Book>>(),
                    It.IsAny<UpdateDefinition<Book>>(),
                    It.IsAny<UpdateOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UpdateResult.Acknowledged(1, 1, null));

            _exportsCollectionMock
                .Setup(x => x.InsertOneAsync(
                    It.IsAny<WarehouseExport>(),
                    It.IsAny<InsertOneOptions>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _exportService.CreateExportAsync(exportCreateDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(exportCreateDto.ExportId, result.ExportId);
            Assert.Equal(exportCreateDto.UserId, result.UserId);
            Assert.Equal(2, result.WarehouseExportBooks.Count);
            Assert.Equal("67eadd6a1af224f20dca6805", result.WarehouseExportBooks[0].BookId);
            Assert.Equal(7, result.WarehouseExportBooks[0].ExportQuantity);
            Assert.Equal("67eadd6a1af224f20dca6809", result.WarehouseExportBooks[1].BookId);
            Assert.Equal(2, result.WarehouseExportBooks[1].ExportQuantity);

            _usersCollectionMock.Verify(x => x.FindAsync(It.IsAny<FilterDefinition<User>>(), It.IsAny<FindOptions<User, User>>(), It.IsAny<CancellationToken>()), Times.Once());
            _booksCollectionMock.Verify(x => x.FindAsync(It.IsAny<FilterDefinition<Book>>(), It.IsAny<FindOptions<Book, Book>>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            _booksCollectionMock.Verify(x => x.UpdateOneAsync(It.IsAny<FilterDefinition<Book>>(), It.IsAny<UpdateDefinition<Book>>(), It.IsAny<UpdateOptions>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            _exportsCollectionMock.Verify(x => x.InsertOneAsync(It.IsAny<WarehouseExport>(), It.IsAny<InsertOneOptions>(), It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task CreateExportAsync_EmptyOrNullWarehouseExportBooks_ThrowsArgumentException()
        {
            // Arrange
            var exportCreateDto1 = new WarehouseExportDto
            {
                ExportId = "67eae55c1af224f20dca6929",
                UserId = "67eacf021af224f20dca689e",
                WarehouseExportBooks = null
            };

            var exportCreateDto2 = new WarehouseExportDto
            {
                ExportId = "67eae55c1af224f20dca6929",
                UserId = "67eacf021af224f20dca689e",
                WarehouseExportBooks = new List<WarehouseExportBookDto>()
            };

            // Act & Assert
            var exception1 = await Assert.ThrowsAsync<ArgumentException>(() => _exportService.CreateExportAsync(exportCreateDto1));
            Assert.Equal("Danh sách sách xuất không được để trống", exception1.Message);

            var exception2 = await Assert.ThrowsAsync<ArgumentException>(() => _exportService.CreateExportAsync(exportCreateDto2));
            Assert.Equal("Danh sách sách xuất không được để trống", exception2.Message);
        }

        [Fact]
        public async Task CreateExportAsync_UserNotFound_ThrowsArgumentException()
        {
            // Arrange
            var exportCreateDto = new WarehouseExportDto
            {
                ExportId = "67eae55c1af224f20dca6929",
                UserId = "non-existent-user",
                WarehouseExportBooks = new List<WarehouseExportBookDto>


                {
                    new WarehouseExportBookDto { BookId = "67eadd6a1af224f20dca6805", ExportQuantity = 7, Price = 4900000 }
                }
            };

            var userCursorMock = new Mock<IAsyncCursor<User>>();
            userCursorMock.Setup(x => x.Current).Returns(new List<User>());
            userCursorMock.SetupSequence(x => x.MoveNext(It.IsAny<CancellationToken>())).Returns(true).Returns(false);
            userCursorMock.SetupSequence(x => x.MoveNextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true).ReturnsAsync(false);

            _usersCollectionMock
                .Setup(x => x.FindAsync(
                    It.IsAny<FilterDefinition<User>>(),
                    It.IsAny<FindOptions<User, User>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(userCursorMock.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _exportService.CreateExportAsync(exportCreateDto));
            Assert.Equal($"Người dùng với UserId {exportCreateDto.UserId} không tồn tại", exception.Message);
        }

        [Fact]
        public async Task CreateExportAsync_BookNotFound_ThrowsArgumentException()
        {
            // Arrange
            var exportCreateDto = new WarehouseExportDto
            {
                ExportId = "67eae55c1af224f20dca6929",
                UserId = "67eacf021af224f20dca689e",
                WarehouseExportBooks = new List<WarehouseExportBookDto>
                {
                    new WarehouseExportBookDto { BookId = "non-existent-book", ExportQuantity = 7, Price = 4900000 }
                }
            };

            var user = new User { UserId = "67eacf021af224f20dca689e", Role = UserRole.STAFF, IsActive = true };

            var userCursorMock = new Mock<IAsyncCursor<User>>();
            userCursorMock.Setup(x => x.Current).Returns(new List<User> { user });
            userCursorMock.SetupSequence(x => x.MoveNext(It.IsAny<CancellationToken>())).Returns(true).Returns(false);
            userCursorMock.SetupSequence(x => x.MoveNextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true).ReturnsAsync(false);

            var bookCursorMock = new Mock<IAsyncCursor<Book>>();
            bookCursorMock.Setup(x => x.Current).Returns(new List<Book>());
            bookCursorMock.SetupSequence(x => x.MoveNext(It.IsAny<CancellationToken>())).Returns(true).Returns(false);
            bookCursorMock.SetupSequence(x => x.MoveNextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true).ReturnsAsync(false);

            _usersCollectionMock
                .Setup(x => x.FindAsync(
                    It.IsAny<FilterDefinition<User>>(),
                    It.IsAny<FindOptions<User, User>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(userCursorMock.Object);

            _booksCollectionMock
                .Setup(x => x.FindAsync(
                    It.IsAny<FilterDefinition<Book>>(),
                    It.IsAny<FindOptions<Book, Book>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(bookCursorMock.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _exportService.CreateExportAsync(exportCreateDto));
            Assert.Equal($"Sách với ID {exportCreateDto.WarehouseExportBooks[0].BookId} không tồn tại", exception.Message);
        }

        [Fact]
        public async Task CreateExportAsync_InsufficientQuantity_ThrowsArgumentException()
        {
            // Arrange
            var exportCreateDto = new WarehouseExportDto
            {
                ExportId = "67eae55c1af224f20dca6929",
                UserId = "67eacf021af224f20dca689e",
                WarehouseExportBooks = new List<WarehouseExportBookDto>
                {
                    new WarehouseExportBookDto { BookId = "67eadd6a1af224f20dca6805", ExportQuantity = 30, Price = 4900000 } // Request 30, but only 20 available
                }
            };

            var user = new User { UserId = "67eacf021af224f20dca689e", Role = UserRole.STAFF, IsActive = true };
            var book = new Book { BookId = "67eadd6a1af224f20dca6805", Quantity = 20 }; // Only 20 in stock

            var userCursorMock = new Mock<IAsyncCursor<User>>();
            userCursorMock.Setup(x => x.Current).Returns(new List<User> { user });
            userCursorMock.SetupSequence(x => x.MoveNext(It.IsAny<CancellationToken>())).Returns(true).Returns(false);
            userCursorMock.SetupSequence(x => x.MoveNextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true).ReturnsAsync(false);

            var bookCursorMock = new Mock<IAsyncCursor<Book>>();
            bookCursorMock.Setup(x => x.Current).Returns(new List<Book> { book });
            bookCursorMock.SetupSequence(x => x.MoveNext(It.IsAny<CancellationToken>())).Returns(true).Returns(false);
            bookCursorMock.SetupSequence(x => x.MoveNextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true).ReturnsAsync(false);

            _usersCollectionMock
                .Setup(x => x.FindAsync(
                    It.IsAny<FilterDefinition<User>>(),
                    It.IsAny<FindOptions<User, User>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(userCursorMock.Object);

            _booksCollectionMock
                .Setup(x => x.FindAsync(
                    It.IsAny<FilterDefinition<Book>>(),
                    It.IsAny<FindOptions<Book, Book>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(bookCursorMock.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _exportService.CreateExportAsync(exportCreateDto));
            Assert.Equal($"Số lượng tồn kho của sách không đủ (còn {book.Quantity})", exception.Message);
        }

        [Fact]
        public async Task GetAllExportsAsync_ReturnsAllExports()
        {
            // Arrange
            var exports = new List<WarehouseExport>
            {
                new WarehouseExport
                {
                    ExportId = "67eae55c1af224f20dca6929",
                    ExportDate = DateTime.Parse("2025-04-02T00:00:00.002Z"),
                    UserId = "67eacf021af224f20dca689e",
                    WarehouseExportBooks = new List<WarehouseExportBook>
                    {
                        new WarehouseExportBook { BookId = "67eadd6a1af224f20dca6805", ExportQuantity = 7 },
                        new WarehouseExportBook { BookId = "67eadd6a1af224f20dca6809", ExportQuantity = 2 }
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

            var exportCursorMock = new Mock<IAsyncCursor<WarehouseExport>>();
            exportCursorMock.Setup(x => x.Current).Returns(exports);
            exportCursorMock.SetupSequence(x => x.MoveNext(It.IsAny<CancellationToken>())).Returns(true).Returns(false);
            exportCursorMock.SetupSequence(x => x.MoveNextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true).ReturnsAsync(false);

            _exportsCollectionMock
                .Setup(x => x.FindAsync(
                    It.IsAny<FilterDefinition<WarehouseExport>>(),
                    It.IsAny<FindOptions<WarehouseExport, WarehouseExport>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(exportCursorMock.Object);

            // Act
            var result = await _exportService.GetAllExportsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("67eae55c1af224f20dca6929", result[0].ExportId);
            Assert.Equal("67eacf021af224f20dca689e", result[0].UserId);
            Assert.Equal(2, result[0].WarehouseExportBooks.Count);
            Assert.Equal("67eae55c1af224f20dca6923", result[1].ExportId);
            Assert.Equal("67eacf021af224f20dca689d", result[1].UserId);
            Assert.Empty(result[1].WarehouseExportBooks);
        }
    }
}