using Bookstore.Application.Dtos;
using Bookstore.Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using MongoDB.Driver;
using Moq;
using Xunit;

namespace Bookstore.Tests.Controllers
{
    public class BookControllerTests
    {
        private readonly Mock<BookService> _bookServiceMock;

        public BookControllerTests()
        {
            _bookServiceMock = new Mock<BookService>(Mock.Of<IMongoDatabase>());
        }

        [Fact]
        public async Task GetAllBooks_ReturnsOk_WithBooks()
        {
            // Arrange
            var books = new List<BookDto> { new() { BookId = "1", Title = "Book 1", Author = "Author 1", Price = 10.5m, CategoryId = "cat1", Quantity = 100 } };
            _bookServiceMock.Setup(s => s.GetAllBooksAsync()).ReturnsAsync(books);
            Func<BookService, Task<IResult>> getAllBooks = async (service) => Results.Ok(await service.GetAllBooksAsync());

            // Act
            var result = await getAllBooks(_bookServiceMock.Object);

            // Assert
            var okResult = Assert.IsType<Ok<List<BookDto>>>(result);
            Assert.Equal(books, okResult.Value);
        }

        [Fact]
        public async Task GetBookById_ReturnsOk_WithBook()
        {
            // Arrange
            var book = new BookDto { BookId = "1", Title = "Book 1", Author = "Author 1", Price = 10.5m, CategoryId = "cat1", Quantity = 100 };
            _bookServiceMock.Setup(s => s.GetBookByIdAsync("1")).ReturnsAsync(book);
            Func<string, BookService, Task<IResult>> getBookById = async (bookId, service) => Results.Ok(await service.GetBookByIdAsync(bookId));

            // Act
            var result = await getBookById("1", _bookServiceMock.Object);

            // Assert
            var okResult = Assert.IsType<Ok<BookDto>>(result);
            Assert.Equal(book, okResult.Value);
        }

        [Fact]
        public async Task CreateBook_ReturnsCreated_WithValidDto()
        {
            // Arrange
            var dto = new BookCreateDto { BookId = "1", Title = "Book 1", Author = "Author 1", Price = 10.5m, CategoryId = "cat1", Quantity = 100 };
            var createdBook = new BookDto { BookId = "1", Title = "Book 1", Author = "Author 1", Price = 10.5m, CategoryId = "cat1", Quantity = 100 };
            _bookServiceMock.Setup(s => s.CreateBookAsync(dto)).ReturnsAsync(createdBook);
            Func<BookCreateDto, BookService, Task<IResult>> createBook = async (dto, service) =>
            {
                var created = await service.CreateBookAsync(dto);
                return Results.Created($"/api/books/{created.BookId}", created);
            };

            // Act
            var result = await createBook(dto, _bookServiceMock.Object);

            // Assert
            var createdResult = Assert.IsType<Created<BookDto>>(result);
            Assert.Equal($"/api/books/{createdBook.BookId}", createdResult.Location);
            Assert.Equal(createdBook, createdResult.Value);
        }

        [Fact]
        public async Task UpdateBook_ReturnsOk_WithValidInput()
        {
            // Arrange
            var dto = new BookUpdateDto { Title = "Updated Book", Author = "Updated Author", Price = 15.0m, CategoryId = "cat1", Quantity = 50 };
            var updatedBook = new BookDto { BookId = "1", Title = "Updated Book", Author = "Updated Author", Price = 15.0m, CategoryId = "cat1", Quantity = 50 };
            _bookServiceMock.Setup(s => s.UpdateBookAsync("1", dto)).ReturnsAsync(updatedBook);
            Func<string, BookUpdateDto, BookService, Task<IResult>> updateBook = async (bookId, dto, service) => Results.Ok(await service.UpdateBookAsync(bookId, dto));

            // Act
            var result = await updateBook("1", dto, _bookServiceMock.Object);

            // Assert
            var okResult = Assert.IsType<Ok<BookDto>>(result);
            Assert.Equal(updatedBook, okResult.Value);
        }

        [Fact]
        public async Task DeleteBook_ReturnsNoContent_WhenSuccessful()
        {
            // Arrange
            _bookServiceMock.Setup(s => s.DeleteBookAsync("1")).ReturnsAsync(true);
            Func<string, BookService, Task<IResult>> deleteBook = async (bookId, service) =>
                await service.DeleteBookAsync(bookId) ? Results.NoContent() : Results.NotFound();

            // Act
            var result = await deleteBook("1", _bookServiceMock.Object);

            // Assert
            Assert.IsType<NoContent>(result);
        }

        [Fact]
        public async Task DeleteBook_ReturnsNotFound_WhenBookDoesNotExist()
        {
            // Arrange
            _bookServiceMock.Setup(s => s.DeleteBookAsync("1")).ReturnsAsync(false);
            Func<string, BookService, Task<IResult>> deleteBook = async (bookId, service) =>
                await service.DeleteBookAsync(bookId) ? Results.NoContent() : Results.NotFound();

            // Act
            var result = await deleteBook("1", _bookServiceMock.Object);

            // Assert
            Assert.IsType<NotFound>(result);
        }
    }
}