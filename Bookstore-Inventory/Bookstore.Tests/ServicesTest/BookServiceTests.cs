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
            var books = new List<Book>
            {
                new Book { BookId = "1", Title = "Book 1", Author = "Author 1", Price = 10, CategoryId = "cat1", Quantity = 5 },
                new Book { BookId = "2", Title = "Book 2", Author = "Author 2", Price = 15, CategoryId = "cat2", Quantity = 10 }
            };

            var findAllBooksMock = new Mock<IAsyncCursor<Book>>();
            findAllBooksMock.Setup(x => x.ToListAsync(It.IsAny<CancellationToken>())).ReturnsAsync(books);

            _booksCollectionMock.Setup(x => x.FindAsync(It.IsAny<FilterDefinition<Book>>(), null, CancellationToken.None))
                .ReturnsAsync(findAllBooksMock.Object);

            var result = await _bookService.GetAllBooksAsync();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains(result, b => b.BookId == "1" && b.Title == "Book 1");
            Assert.Contains(result, b => b.BookId == "2" && b.Title == "Book 2");
        }

        // Test GetBookByIdAsync
        [Fact]
        public async Task GetBookByIdAsync_ValidId_ReturnsBook()
        {
            var bookId = "1";
            var book = new Book
            {
                BookId = bookId,
                Title = "Book 1",
                Author = "Author 1",
                Price = 10,
                CategoryId = "cat1",
                Quantity = 5
            };

            var findBookMock = new Mock<IAsyncCursor<Book>>();
            findBookMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<CancellationToken>())).ReturnsAsync(book);

            _booksCollectionMock.Setup(x => x.FindAsync(It.IsAny<FilterDefinition<Book>>(), null, CancellationToken.None))
                .ReturnsAsync(findBookMock.Object);

            var result = await _bookService.GetBookByIdAsync(bookId);

            Assert.NotNull(result);
            Assert.Equal(book.BookId, result.BookId);
            Assert.Equal(book.Title, result.Title);
            Assert.Equal(book.Author, result.Author);
            Assert.Equal(book.Price, result.Price);
            Assert.Equal(book.CategoryId, result.CategoryId);
            Assert.Equal(book.Quantity, result.Quantity);
        }

        [Fact]
        public async Task GetBookByIdAsync_NullOrEmptyId_ThrowsArgumentException()
        {
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _bookService.GetBookByIdAsync(""));
            Assert.Equal("BookId không được để trống", exception.Message);
        }

        [Fact]
        public async Task GetBookByIdAsync_BookNotFound_ThrowsArgumentException()
        {
            var bookId = "1";
            var findBookMock = new Mock<IAsyncCursor<Book>>();
            findBookMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<CancellationToken>())).ReturnsAsync((Book)null);

            _booksCollectionMock.Setup(x => x.FindAsync(It.IsAny<FilterDefinition<Book>>(), null, CancellationToken.None))
                .ReturnsAsync(findBookMock.Object);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _bookService.GetBookByIdAsync(bookId));
            Assert.Equal($"Sách với ID {bookId} không tồn tại", exception.Message);
        }

        // Test CreateBookAsync
        [Fact]
        public async Task CreateBookAsync_ValidBook_CreatesBook()
        {
            var bookCreateDto = new BookCreateDto
            {
                BookId = "1",
                Title = "New Book",
                Author = "New Author",
                Price = 20,
                CategoryId = "cat1",
                Quantity = 3
            };

            var findBookMock = new Mock<IAsyncCursor<Book>>();
            findBookMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<CancellationToken>())).ReturnsAsync((Book)null);

            _booksCollectionMock.Setup(x => x.FindAsync(It.IsAny<FilterDefinition<Book>>(), null, CancellationToken.None))
                .ReturnsAsync(findBookMock.Object);
            _booksCollectionMock.Setup(x => x.InsertOneAsync(It.IsAny<Book>(), null, CancellationToken.None))
                .Returns(Task.CompletedTask);

            var result = await _bookService.CreateBookAsync(bookCreateDto);

            Assert.NotNull(result);
            Assert.Equal(bookCreateDto.BookId, result.BookId);
            Assert.Equal(bookCreateDto.Title, result.Title);
            Assert.Equal(bookCreateDto.Author, result.Author);
            Assert.Equal(bookCreateDto.Price, result.Price);
            Assert.Equal(bookCreateDto.CategoryId, result.CategoryId);
            Assert.Equal(bookCreateDto.Quantity, result.Quantity);
            _booksCollectionMock.Verify(x => x.InsertOneAsync(It.IsAny<Book>(), null, CancellationToken.None), Times.Once());
        }

        [Fact]
        public async Task CreateBookAsync_DuplicateBookId_ThrowsArgumentException()
        {
            var bookCreateDto = new BookCreateDto { BookId = "1", Title = "New Book" };
            var existingBook = new Book { BookId = "1" };

            var findBookMock = new Mock<IAsyncCursor<Book>>();
            findBookMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<CancellationToken>())).ReturnsAsync(existingBook);

            _booksCollectionMock.Setup(x => x.FindAsync(It.IsAny<FilterDefinition<Book>>(), null, CancellationToken.None))
                .ReturnsAsync(findBookMock.Object);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _bookService.CreateBookAsync(bookCreateDto));
            Assert.Equal($"Sách với BookId {bookCreateDto.BookId} đã tồn tại", exception.Message);
        }

        // Test UpdateBookAsync
        [Fact]
        public async Task UpdateBookAsync_ValidBook_UpdatesBook()
        {
            var bookId = "1";
            var bookUpdateDto = new BookUpdateDto
            {
                Title = "Updated Book",
                Author = "Updated Author",
                Price = 25,
                CategoryId = "cat2",
                Quantity = 8
            };
            var existingBook = new Book { BookId = bookId, Title = "Old Book", Author = "Old Author", Price = 10, CategoryId = "cat1", Quantity = 5 };

            var findBookMock = new Mock<IAsyncCursor<Book>>();
            findBookMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<CancellationToken>())).ReturnsAsync(existingBook);

            _booksCollectionMock.Setup(x => x.FindAsync(It.IsAny<FilterDefinition<Book>>(), null, CancellationToken.None))
                .ReturnsAsync(findBookMock.Object);
            _booksCollectionMock.Setup(x => x.ReplaceOneAsync(It.IsAny<FilterDefinition<Book>>(), It.IsAny<Book>(), It.IsAny<ReplaceOptions>(), CancellationToken.None))
                .ReturnsAsync(new ReplaceOneResult.Acknowledged(1, 1, null));

            var result = await _bookService.UpdateBookAsync(bookId, bookUpdateDto);

            Assert.NotNull(result);
            Assert.Equal(bookId, result.BookId);
            Assert.Equal(bookUpdateDto.Title, result.Title);
            Assert.Equal(bookUpdateDto.Author, result.Author);
            Assert.Equal(bookUpdateDto.Price, result.Price);
            Assert.Equal(bookUpdateDto.CategoryId, result.CategoryId);
            Assert.Equal(bookUpdateDto.Quantity, result.Quantity);
            _booksCollectionMock.Verify(x => x.ReplaceOneAsync(It.IsAny<FilterDefinition<Book>>(), It.IsAny<Book>(), It.IsAny<ReplaceOptions>(), CancellationToken.None), Times.Once());
        }

        [Fact]
        public async Task UpdateBookAsync_NullOrEmptyId_ThrowsArgumentException()
        {
            var bookUpdateDto = new BookUpdateDto { Title = "Updated Book" };

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _bookService.UpdateBookAsync("", bookUpdateDto));
            Assert.Equal("BookId không được để trống", exception.Message);
        }

        [Fact]
        public async Task UpdateBookAsync_BookNotFound_ThrowsArgumentException()
        {
            var bookId = "1";
            var bookUpdateDto = new BookUpdateDto { Title = "Updated Book" };

            var findBookMock = new Mock<IAsyncCursor<Book>>();
            findBookMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<CancellationToken>())).ReturnsAsync((Book)null);

            _booksCollectionMock.Setup(x => x.FindAsync(It.IsAny<FilterDefinition<Book>>(), null, CancellationToken.None))
                .ReturnsAsync(findBookMock.Object);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _bookService.UpdateBookAsync(bookId, bookUpdateDto));
            Assert.Equal($"Sách với ID {bookId} không tồn tại", exception.Message);
        }

        // Test DeleteBookAsync
        [Fact]
        public async Task DeleteBookAsync_ValidId_DeletesBook()
        {
            var bookId = "1";

            _booksCollectionMock.Setup(x => x.DeleteOneAsync(It.IsAny<FilterDefinition<Book>>(), CancellationToken.None))
                .ReturnsAsync(new DeleteResult.Acknowledged(1));

            var result = await _bookService.DeleteBookAsync(bookId);

            Assert.True(result);
            _booksCollectionMock.Verify(x => x.DeleteOneAsync(It.IsAny<FilterDefinition<Book>>(), CancellationToken.None), Times.Once());
        }

        [Fact]
        public async Task DeleteBookAsync_NullOrEmptyId_ThrowsArgumentException()
        {
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _bookService.DeleteBookAsync(""));
            Assert.Equal("BookId không được để trống", exception.Message);
        }

        [Fact]
        public async Task DeleteBookAsync_BookNotFound_ReturnsFalse()
        {
            var bookId = "1";

            _booksCollectionMock.Setup(x => x.DeleteOneAsync(It.IsAny<FilterDefinition<Book>>(), CancellationToken.None))
                .ReturnsAsync(new DeleteResult.Acknowledged(0));

            var result = await _bookService.DeleteBookAsync(bookId);

            Assert.False(result);
        }
    }
}