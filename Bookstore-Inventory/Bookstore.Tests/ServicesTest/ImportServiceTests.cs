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
    public class ImportServiceTests
    {
        private readonly Mock<IMongoCollection<WarehouseImport>> _importsCollectionMock;
        private readonly Mock<IMongoCollection<Book>> _booksCollectionMock;
        private readonly Mock<IMongoCollection<User>> _usersCollectionMock;
        private readonly ImportService _importService;

        public ImportServiceTests()
        {
            var databaseMock = new Mock<IMongoDatabase>();
            _importsCollectionMock = new Mock<IMongoCollection<WarehouseImport>>();
            _booksCollectionMock = new Mock<IMongoCollection<Book>>();
            _usersCollectionMock = new Mock<IMongoCollection<User>>();

            databaseMock.Setup(db => db.GetCollection<WarehouseImport>("WarehouseImport", null)).Returns(_importsCollectionMock.Object);
            databaseMock.Setup(db => db.GetCollection<Book>("Book", null)).Returns(_booksCollectionMock.Object);
            databaseMock.Setup(db => db.GetCollection<User>("User", null)).Returns(_usersCollectionMock.Object);

            _importService = new ImportService(databaseMock.Object);
        }

        [Fact]
        public async Task CreateImportAsync_ValidImport_CreatesImportAndUpdatesBooks()
        {
            // Arrange
            var importCreateDto = new WarehouseImportDto
            {
                ImportId = "67eae6131af224f20dca6931", // From database sample
                ImportDate = DateTime.Parse("2025-04-01T00:00:00.002Z"),
                UserId = "67eacf021af224f20dca689d", // From database sample
                WarehouseImportBooks = new List<WarehouseImportBookDto>
                {
                    new WarehouseImportBookDto { BookId = "67eadd6a1af224f20dca68cd", ImportQuantity = 15, Price = 3500000 }, // From database sample
                    new WarehouseImportBookDto { BookId = "67eadd6a1af224f20dca68ce", ImportQuantity = 12, Price = 3700000 } // From database sample
                }
            };

            var user = new User { UserId = "67eacf021af224f20dca689d", Role = UserRole.ADMINISTRATOR, IsActive = true };
            var book1 = new Book { BookId = "67eadd6a1af224f20dca68cd", Quantity = 10 };
            var book2 = new Book { BookId = "67eadd6a1af224f20dca68ce", Quantity = 15 };

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

            _importsCollectionMock
                .Setup(x => x.InsertOneAsync(
                    It.IsAny<WarehouseImport>(),
                    It.IsAny<InsertOneOptions>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _importService.CreateImportAsync(importCreateDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(importCreateDto.ImportId, result.ImportId);
            Assert.Equal(importCreateDto.UserId, result.UserId);
            Assert.Equal(2, result.WarehouseImportBooks.Count);
            Assert.Equal("67eadd6a1af224f20dca68cd", result.WarehouseImportBooks[0].BookId);
            Assert.Equal(15, result.WarehouseImportBooks[0].ImportQuantity);
            Assert.Equal("67eadd6a1af224f20dca68ce", result.WarehouseImportBooks[1].BookId);
            Assert.Equal(12, result.WarehouseImportBooks[1].ImportQuantity);

            _usersCollectionMock.Verify(x => x.FindAsync(It.IsAny<FilterDefinition<User>>(), It.IsAny<FindOptions<User, User>>(), It.IsAny<CancellationToken>()), Times.Once());
            _booksCollectionMock.Verify(x => x.FindAsync(It.IsAny<FilterDefinition<Book>>(), It.IsAny<FindOptions<Book, Book>>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            _booksCollectionMock.Verify(x => x.UpdateOneAsync(It.IsAny<FilterDefinition<Book>>(), It.IsAny<UpdateDefinition<Book>>(), It.IsAny<UpdateOptions>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            _importsCollectionMock.Verify(x => x.InsertOneAsync(It.IsAny<WarehouseImport>(), It.IsAny<InsertOneOptions>(), It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task CreateImportAsync_EmptyOrNullWarehouseImportBooks_ThrowsArgumentException()
        {
            // Arrange
            var importCreateDto1 = new WarehouseImportDto
            {
                ImportId = "67eae6131af224f20dca6931",
                UserId = "67eacf021af224f20dca689d",
                WarehouseImportBooks = null
            };

            var importCreateDto2 = new WarehouseImportDto
            {
                ImportId = "67eae6131af224f20dca6931",
                UserId = "67eacf021af224f20dca689d",
                WarehouseImportBooks = new List<WarehouseImportBookDto>()
            };

            // Act & Assert
            var exception1 = await Assert.ThrowsAsync<ArgumentException>(() => _importService.CreateImportAsync(importCreateDto1));
            Assert.Equal("Danh sách sách nhập không được để trống", exception1.Message);

            var exception2 = await Assert.ThrowsAsync<ArgumentException>(() => _importService.CreateImportAsync(importCreateDto2));
            Assert.Equal("Danh sách sách nhập không được để trống", exception2.Message);
        }

        [Fact]
        public async Task CreateImportAsync_UserNotFound_ThrowsArgumentException()
        {
            // Arrange
            var importCreateDto = new WarehouseImportDto
            {
                ImportId = "67eae6131af224f20dca6931",
                UserId = "non-existent-user",
                WarehouseImportBooks = new List<WarehouseImportBookDto>
                {
                    new WarehouseImportBookDto { BookId = "67eadd6a1af224f20dca68cd", ImportQuantity = 15, Price = 3500000 }
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
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _importService.CreateImportAsync(importCreateDto));
            Assert.Equal($"Người dùng với UserId {importCreateDto.UserId} không tồn tại", exception.Message);
        }

        [Fact]
        public async Task CreateImportAsync_BookNotFound_ThrowsArgumentException()
        {
            // Arrange
            var importCreateDto = new WarehouseImportDto
            {
                ImportId = "67eae6131af224f20dca6931",
                UserId = "67eacf021af224f20dca689d",
                WarehouseImportBooks = new List<WarehouseImportBookDto>
                {
                    new WarehouseImportBookDto { BookId = "non-existent-book", ImportQuantity = 15, Price = 3500000 }
                }
            };

            var user = new User { UserId = "67eacf021af224f20dca689d", Role = UserRole.ADMINISTRATOR, IsActive = true };

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
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _importService.CreateImportAsync(importCreateDto));
            Assert.Equal($"Sách với ID {importCreateDto.WarehouseImportBooks[0].BookId} không tồn tại", exception.Message);
        }

        [Fact]
        public async Task GetAllImportsAsync_ReturnsAllImports()
        {
            // Arrange
            var imports = new List<WarehouseImport>
            {
                new WarehouseImport
                {
                    ImportId = "67eae6131af224f20dca6931",
                    ImportDate = DateTime.Parse("2025-04-01T00:00:00.002Z"),
                    UserId = "67eacf021af224f20dca689d",
                    WarehouseImportBooks = new List<WarehouseImportBook>
                    {
                        new WarehouseImportBook { BookId = "67eadd6a1af224f20dca68cd", ImportQuantity = 15, Price = 3500000 },
                        new WarehouseImportBook { BookId = "67eadd6a1af224f20dca68ce", ImportQuantity = 12, Price = 3700000 }
                    }
                },
                new WarehouseImport
                {
                    ImportId = "67eae6131af224f20dca6935",
                    ImportDate = DateTime.Parse("2025-04-01T00:00:00.002Z"),
                    UserId = "67eacf021af224f20dca689e",
                    WarehouseImportBooks = new List<WarehouseImportBook>()
                }
            };

            var importCursorMock = new Mock<IAsyncCursor<WarehouseImport>>();
            importCursorMock.Setup(x => x.Current).Returns(imports);
            importCursorMock.SetupSequence(x => x.MoveNext(It.IsAny<CancellationToken>())).Returns(true).Returns(false);
            importCursorMock.SetupSequence(x => x.MoveNextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true).ReturnsAsync(false);

            _importsCollectionMock
                .Setup(x => x.FindAsync(
                    It.IsAny<FilterDefinition<WarehouseImport>>(),
                    It.IsAny<FindOptions<WarehouseImport, WarehouseImport>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(importCursorMock.Object);

            // Act
            var result = await _importService.GetAllImportsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("67eae6131af224f20dca6931", result[0].ImportId);
            Assert.Equal("67eacf021af224f20dca689d", result[0].UserId);
            Assert.Equal(2, result[0].WarehouseImportBooks.Count);
            Assert.Equal("67eae6131af224f20dca6935", result[1].ImportId);
            Assert.Equal("67eacf021af224f20dca689e", result[1].UserId);
            Assert.Empty(result[1].WarehouseImportBooks);
        }
    }
}