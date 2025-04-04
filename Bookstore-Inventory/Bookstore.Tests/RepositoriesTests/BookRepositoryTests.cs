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
    public class BookRepositoryTests
    {
        private readonly Mock<IMongoCollection<Book>> _mockCollection;
        private readonly Mock<MongoDbContext> _mockDbContext;
        private readonly BookRepository _repository;

        public BookRepositoryTests()
        {
            // Khởi tạo mock cho IMongoCollection
            _mockCollection = new Mock<IMongoCollection<Book>>();

            // Khởi tạo mock cho MongoDbContext
            _mockDbContext = new Mock<MongoDbContext>();
            _mockDbContext
                .Setup(db => db.GetCollection<Book>(It.IsAny<string>()))
                .Returns(_mockCollection.Object);

            // Khởi tạo repository
            _repository = new BookRepository(_mockDbContext.Object);
        }

        [Fact]
        public async Task GetAllBooksAsync_ShouldReturnAllBooks()
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

            _mockCollection
                .Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<Book>>(),
                    It.IsAny<FindOptions<Book, Book>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            // Act
            var result = await _repository.GetAllBooksAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Equal("Code Complete", result[0].Title);
            Assert.Equal("The Art of Computer Programming", result[1].Title);
            Assert.Equal("Artificial Intelligence: A Modern Approach", result[2].Title);
        }

        [Fact]
        public async Task GetBookByIdAsync_ShouldReturnBook_WhenIdExists()
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

            _mockCollection
                .Setup(c => c.FindAsync(
                    It.Is<FilterDefinition<Book>>(f => f == Builders<Book>.Filter.Eq(x => x.BookId, bookId)),
                    It.IsAny<FindOptions<Book, Book>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            // Act
            var result = await _repository.GetBookByIdAsync(bookId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(bookId, result.BookId);
            Assert.Equal("Code Complete", result.Title);
            Assert.Equal("Steve McConnell", result.Author);
            Assert.Equal(580000, result.Price);
            Assert.Equal(70, result.Quantity);
            Assert.Equal("67eacc121af224f20dca6882", result.CategoryId);
        }

        [Fact]
        public async Task GetBookByIdAsync_ShouldReturnNull_WhenIdDoesNotExist()
        {
            // Arrange
            var bookId = "non-existent-id";

            var mockCursor = new Mock<IAsyncCursor<Book>>();
            mockCursor.Setup(_ => _.Current).Returns(new List<Book>());
            mockCursor
                .SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockCollection
                .Setup(c => c.FindAsync(
                    It.Is<FilterDefinition<Book>>(f => f == Builders<Book>.Filter.Eq(x => x.BookId, bookId)),
                    It.IsAny<FindOptions<Book, Book>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            // Act
            var result = await _repository.GetBookByIdAsync(bookId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AddBookAsync_ShouldInsertBook()
        {
            // Arrange
            var book = new Book
            {
                BookId = "67eadd61af224f20dca68d4",
                Title = "Code Complete",
                Author = "Steve McConnell",
                Price = 580000,
                Quantity = 70,
                CategoryId = "67eacc121af224f20dca6882"
            };

            _mockCollection
                .Setup(c => c.InsertOneAsync(
                    It.IsAny<Book>(),
                    It.IsAny<InsertOneOptions>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _repository.AddBookAsync(book);

            // Assert
            _mockCollection.Verify(c => c.InsertOneAsync(book, null, default), Times.Once());
        }

        [Fact]
        public async Task UpdateBookAsync_ShouldReplaceBook()
        {
            // Arrange
            var book = new Book
            {
                BookId = "67eadd61af224f20dca68d4",
                Title = "Code Complete Updated",
                Author = "Steve McConnell",
                Price = 600000,
                Quantity = 80,
                CategoryId = "67eacc121af224f20dca6882"
            };

            _mockCollection
                .Setup(c => c.ReplaceOneAsync(
                    It.IsAny<FilterDefinition<Book>>(),
                    It.IsAny<Book>(),
                    It.IsAny<ReplaceOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ReplaceOneResult.Acknowledged(1, 1, null));

            // Act
            await _repository.UpdateBookAsync(book);

            // Assert
            _mockCollection.Verify(c => c.ReplaceOneAsync(
                It.IsAny<FilterDefinition<Book>>(),
                book,
                It.IsAny<ReplaceOptions>(),
                default), Times.Once());
        }

        [Fact]
        public async Task DeleteBookAsync_ShouldRemoveBook()
        {
            // Arrange
            var bookId = "67eadd61af224f20dca68d4";

            _mockCollection
                .Setup(c => c.DeleteOneAsync(
                    It.IsAny<FilterDefinition<Book>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DeleteResult.Acknowledged(1));

            // Act
            await _repository.DeleteBookAsync(bookId);

            // Assert
            _mockCollection.Verify(c => c.DeleteOneAsync(
                It.IsAny<FilterDefinition<Book>>(),
                default), Times.Once());
        }
    }
}