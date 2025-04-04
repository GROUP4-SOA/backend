using Bookstore.Application.Dtos;
using Bookstore.Application.Services;
using Bookstore.Domain.Entities;
using MongoDB.Driver;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Bookstore.Tests
{
    public class BookServiceTests
    {
        private readonly Mock<IMongoCollection<Book>> _booksCollectionMock;
        private readonly BookService _bookService;

        public BookServiceTests()
        {
            var databaseMock = new Mock<IMongoDatabase>();
            _booksCollectionMock = new Mock<IMongoCollection<Book>>();

            databaseMock.Setup(db => db.GetCollection<Book>("Book", null))
                .Returns(_booksCollectionMock.Object);

            _bookService = new BookService(databaseMock.Object);
        }

        // Test GetAllBooksAsync
        [Fact]
        public async Task GetAllBooksAsync_ReturnsAllBooks()
        {
            // Arrange
            var books = new List<Book>
            {
                new Book
                {
                    BookId = "67eadd61af224f20dca68d4",
                    Title = "Code Complete",
                    Author = "Steve McConnell",
                    Price = 580000,
                    Quantity = 70,
                    CategoryId = "67eacc121af224f20dca6882"
                },
                new Book
                {
                    BookId = "67eadd61af224f20dca68d6",
                    Title = "The Art of Computer Programming",
                    Author = "Donald Knuth",
                    Price = 500000,
                    Quantity = 95,
                    CategoryId = "67eacc121af224f20dca6882"
                },
                new Book
                {
                    BookId = "67eadd61af224f20dca68d9",
                    Title = "Artificial Intelligence: A Modern Approach",
                    Author = "Stuart Russell & Peter Norvig",
                    Price = 550000,
                    Quantity = 90,
                    CategoryId = "67eacc121af224f20dca6887"
                }
            };

            var mockCursor = new Mock<IAsyncCursor<Book>>();
            mockCursor.Setup(_ => _.Current).Returns(books);
            mockCursor
                .SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            _booksCollectionMock
                .Setup(x => x.FindAsync(
                    It.IsAny<FilterDefinition<Book>>(),
                    It.IsAny<FindOptions<Book, Book>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            // Act
            var result = await _bookService.GetAllBooksAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Contains(result, b => b.BookId == "67eadd61af224f20dca68d4" && b.Title == "Code Complete" && b.Author == "Steve McConnell" && b.Price == 580000 && b.Quantity == 70 && b.CategoryId == "67eacc121af224f20dca6882");
            Assert.Contains(result, b => b.BookId == "67eadd61af224f20dca68d6" && b.Title == "The Art of Computer Programming" && b.Author == "Donald Knuth" && b.Price == 500000 && b.Quantity == 95 && b.CategoryId == "67eacc121af224f20dca6882");
            Assert.Contains(result, b => b.BookId == "67eadd61af224f20dca68d9" && b.Title == "Artificial Intelligence: A Modern Approach" && b.Author == "Stuart Russell & Peter Norvig" && b.Price == 550000 && b.Quantity == 90 && b.CategoryId == "67eacc121af224f20dca6887");
        }

        // Test GetBookByIdAsync
        [Fact]
        public async Task GetBookByIdAsync_ValidId_ReturnsBook()
        {
            // Arrange
            var bookId = "67eadd61af224f20dca68d4";
            var book = new Book
            {
                BookId = bookId,
                Title = "Code Complete",
                Author = "Steve McConnell",
                Price = 580000,
                Quantity = 70,
                CategoryId = "67eacc121af224f20dca6882"
            };

            var mockCursor = new Mock<IAsyncCursor<Book>>();
            mockCursor.Setup(_ => _.Current).Returns(new List<Book> { book });
            mockCursor
                .SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            _booksCollectionMock
                .Setup(x => x.FindAsync(
                    It.IsAny<FilterDefinition<Book>>(),
                    It.IsAny<FindOptions<Book, Book>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            // Act
            var result = await _bookService.GetBookByIdAsync(bookId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(book.BookId, result.BookId);
            Assert.Equal(book.Title, result.Title);
            Assert.Equal(book.Author, result.Author);
            Assert.Equal(book.Price, result.Price);
            Assert.Equal(book.Quantity, result.Quantity);
            Assert.Equal(book.CategoryId, result.CategoryId);
        }

        [Fact]
        public async Task GetBookByIdAsync_NullOrEmptyId_ThrowsArgumentException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _bookService.GetBookByIdAsync(""));
            Assert.Equal("BookId không được để trống", exception.Message);
        }

        [Fact]
        public async Task GetBookByIdAsync_BookNotFound_ThrowsArgumentException()
        {
            // Arrange
            var bookId = "non-existent-id";

            var mockCursor = new Mock<IAsyncCursor<Book>>();
            mockCursor.Setup(_ => _.Current).Returns(new List<Book>());
            mockCursor
                .SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _booksCollectionMock
                .Setup(x => x.FindAsync(
                    It.IsAny<FilterDefinition<Book>>(),
                    It.IsAny<FindOptions<Book, Book>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _bookService.GetBookByIdAsync(bookId));
            Assert.Equal($"Sách với ID {bookId} không tồn tại", exception.Message);
        }

        // Test CreateBookAsync
        [Fact]
        public async Task CreateBookAsync_ValidBook_CreatesBook()
        {
            // Arrange
            var bookCreateDto = new BookCreateDto
            {
                BookId = "67eadd61af224f20dca68e0",
                Title = "New Book",
                Author = "New Author",
                Price = 600000,
                Quantity = 50,
                CategoryId = "67eacc121af224f20dca6882"
            };

            var mockCursor = new Mock<IAsyncCursor<Book>>();
            mockCursor.Setup(_ => _.Current).Returns(new List<Book>());
            mockCursor
                .SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _booksCollectionMock
                .Setup(x => x.FindAsync(
                    It.IsAny<FilterDefinition<Book>>(),
                    It.IsAny<FindOptions<Book, Book>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            _booksCollectionMock
                .Setup(x => x.InsertOneAsync(
                    It.IsAny<Book>(),
                    It.IsAny<InsertOneOptions>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _bookService.CreateBookAsync(bookCreateDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(bookCreateDto.BookId, result.BookId);
            Assert.Equal(bookCreateDto.Title, result.Title);
            Assert.Equal(bookCreateDto.Author, result.Author);
            Assert.Equal(bookCreateDto.Price, result.Price);
            Assert.Equal(bookCreateDto.Quantity, result.Quantity);
            Assert.Equal(bookCreateDto.CategoryId, result.CategoryId);
            _booksCollectionMock.Verify(x => x.InsertOneAsync(It.IsAny<Book>(), null, CancellationToken.None), Times.Once());
        }

        [Fact]
        public async Task CreateBookAsync_DuplicateBookId_ThrowsArgumentException()
        {
            // Arrange
            var bookCreateDto = new BookCreateDto
            {
                BookId = "67eadd61af224f20dca68d4",
                Title = "Code Complete",
                Author = "Steve McConnell",
                Price = 580000,
                Quantity = 70,
                CategoryId = "67eacc121af224f20dca6882"
            };

            var existingBook = new Book { BookId = bookCreateDto.BookId };

            var mockCursor = new Mock<IAsyncCursor<Book>>();
            mockCursor.Setup(_ => _.Current).Returns(new List<Book> { existingBook });
            mockCursor
                .SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            _booksCollectionMock
                .Setup(x => x.FindAsync(
                    It.IsAny<FilterDefinition<Book>>(),
                    It.IsAny<FindOptions<Book, Book>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _bookService.CreateBookAsync(bookCreateDto));
            Assert.Equal($"Sách với BookId {bookCreateDto.BookId} đã tồn tại", exception.Message);
        }

        // Test UpdateBookAsync
        [Fact]
        public async Task UpdateBookAsync_ValidBook_UpdatesBook()
        {
            // Arrange
            var bookId = "67eadd61af224f20dca68d4";
            var bookUpdateDto = new BookUpdateDto
            {
                Title = "Updated Code Complete",
                Author = "Steve McConnell",
                Price = 600000,
                Quantity = 80,
                CategoryId = "67eacc121af224f20dca6882"
            };
            var existingBook = new Book
            {
                BookId = bookId,
                Title = "Code Complete",
                Author = "Steve McConnell",
                Price = 580000,
                Quantity = 70,
                CategoryId = "67eacc121af224f20dca6882"
            };

            var mockCursor = new Mock<IAsyncCursor<Book>>();
            mockCursor.Setup(_ => _.Current).Returns(new List<Book> { existingBook });
            mockCursor
                .SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            _booksCollectionMock
                .Setup(x => x.FindAsync(
                    It.IsAny<FilterDefinition<Book>>(),
                    It.IsAny<FindOptions<Book, Book>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            _booksCollectionMock
                .Setup(x => x.ReplaceOneAsync(
                    It.IsAny<FilterDefinition<Book>>(),
                    It.IsAny<Book>(),
                    It.IsAny<ReplaceOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ReplaceOneResult.Acknowledged(1, 1, null));

            // Act
            var result = await _bookService.UpdateBookAsync(bookId, bookUpdateDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(bookId, result.BookId);
            Assert.Equal(bookUpdateDto.Title, result.Title);
            Assert.Equal(bookUpdateDto.Author, result.Author);
            Assert.Equal(bookUpdateDto.Price, result.Price);
            Assert.Equal(bookUpdateDto.Quantity, result.Quantity);
            Assert.Equal(bookUpdateDto.CategoryId, result.CategoryId);
            _booksCollectionMock.Verify(x => x.ReplaceOneAsync(It.IsAny<FilterDefinition<Book>>(), It.IsAny<Book>(), It.IsAny<ReplaceOptions>(), CancellationToken.None), Times.Once());
        }

        [Fact]
        public async Task UpdateBookAsync_NullOrEmptyId_ThrowsArgumentException()
        {
            // Arrange
            var bookUpdateDto = new BookUpdateDto
            {
                Title = "Updated Code Complete",
                Author = "Steve McConnell",
                Price = 600000,
                Quantity = 80,
                CategoryId = "67eacc121af224f20dca6882"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _bookService.UpdateBookAsync("", bookUpdateDto));
            Assert.Equal("BookId không được để trống", exception.Message);
        }

        [Fact]
        public async Task UpdateBookAsync_BookNotFound_ThrowsArgumentException()
        {
            // Arrange
            var bookId = "non-existent-id";
            var bookUpdateDto = new BookUpdateDto
            {
                Title = "Updated Code Complete",
                Author = "Steve McConnell",
                Price = 600000,
                Quantity = 80,
                CategoryId = "67eacc121af224f20dca6882"
            };

            var mockCursor = new Mock<IAsyncCursor<Book>>();
            mockCursor.Setup(_ => _.Current).Returns(new List<Book>());
            mockCursor
                .SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _booksCollectionMock
                .Setup(x => x.FindAsync(
                    It.IsAny<FilterDefinition<Book>>(),
                    It.IsAny<FindOptions<Book, Book>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _bookService.UpdateBookAsync(bookId, bookUpdateDto));
            Assert.Equal($"Sách với ID {bookId} không tồn tại", exception.Message);
        }

        // Test DeleteBookAsync
        [Fact]
        public async Task DeleteBookAsync_ValidId_DeletesBook()
        {
            // Arrange
            var bookId = "67eadd61af224f20dca68d4";

            _booksCollectionMock
                .Setup(x => x.DeleteOneAsync(
                    It.IsAny<FilterDefinition<Book>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DeleteResult.Acknowledged(1));

            // Act
            var result = await _bookService.DeleteBookAsync(bookId);

            // Assert
            Assert.True(result);
            _booksCollectionMock.Verify(x => x.DeleteOneAsync(It.IsAny<FilterDefinition<Book>>(), CancellationToken.None), Times.Once());
        }

        [Fact]
        public async Task DeleteBookAsync_NullOrEmptyId_ThrowsArgumentException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _bookService.DeleteBookAsync(""));
            Assert.Equal("BookId không được để trống", exception.Message);
        }

        [Fact]
        public async Task DeleteBookAsync_BookNotFound_ReturnsFalse()
        {
            // Arrange
            var bookId = "non-existent-id";

            _booksCollectionMock
                .Setup(x => x.DeleteOneAsync(
                    It.IsAny<FilterDefinition<Book>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DeleteResult.Acknowledged(0));

            // Act
            var result = await _bookService.DeleteBookAsync(bookId);

            // Assert
            Assert.False(result);
        }
    }
}