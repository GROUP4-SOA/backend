using Bookstore.Application.Dtos;
using Bookstore.Application.Interfaces.Services;
using Bookstore.API.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Bookstore.Tests.Controllers
{
    public class BookControllerTests
    {
        private readonly Mock<IBookService> _bookServiceMock;
        private readonly BookController _controller;

        public BookControllerTests()
        {
            _bookServiceMock = new Mock<IBookService>();
            _controller = new BookController(_bookServiceMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
        }

        [Fact]
        public async Task GetAllBooks_ReturnsOk_WithBooks()
        {
            // Arrange
            var books = new List<BookDto>
            {
                new BookDto { BookId = "67eadd6a1af224f20dca684d", Title = "Code Complete", Author = "Steve McConnell", Price = 580000, Quantity = 70, CategoryId = "67eacc121af224f20dca6882" },
                new BookDto { BookId = "67eadd6a1af224f20dca6846", Title = "The Art of Computer Programming", Author = "Donald Knuth", Price = 500000, Quantity = 95, CategoryId = "67eacc121af224f20dca6882" },
                new BookDto { BookId = "67eadd6a1af224f20dca6849", Title = "Artificial Intelligence: A Modern Approach", Author = "Stuart Russell & Peter Norvig", Price = 550000, Quantity = 90, CategoryId = "67eacc121af224f20dca6887" }
            };
            _bookServiceMock.Setup(s => s.GetAllBooksAsync()).ReturnsAsync(books);

            // Act
            var result = await _controller.GetAllBooks();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedBooks = Assert.IsType<List<BookDto>>(okResult.Value);
            Assert.Equal(books.Count, returnedBooks.Count);
            for (int i = 0; i < books.Count; i++)
            {
                Assert.Equal(books[i].BookId, returnedBooks[i].BookId);
                Assert.Equal(books[i].Title, returnedBooks[i].Title);
                Assert.Equal(books[i].Author, returnedBooks[i].Author);
                Assert.Equal(books[i].Price, returnedBooks[i].Price);
                Assert.Equal(books[i].Quantity, returnedBooks[i].Quantity);
                Assert.Equal(books[i].CategoryId, returnedBooks[i].CategoryId);
            }
        }

        [Fact]
        public async Task GetBookById_ReturnsOk_WithBook()
        {
            // Arrange
            var bookId = "67eadd6a1af224f20dca684d";
            var book = new BookDto { BookId = bookId, Title = "Code Complete", Author = "Steve McConnell", Price = 580000, Quantity = 70, CategoryId = "67eacc121af224f20dca6882" };
            _bookServiceMock.Setup(s => s.GetBookByIdAsync(bookId)).ReturnsAsync(book);

            // Act
            var result = await _controller.GetBookById(bookId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedBook = Assert.IsType<BookDto>(okResult.Value);
            Assert.Equal(book.BookId, returnedBook.BookId);
            Assert.Equal(book.Title, returnedBook.Title);
            Assert.Equal(book.Author, returnedBook.Author);
            Assert.Equal(book.Price, returnedBook.Price);
            Assert.Equal(book.Quantity, returnedBook.Quantity);
            Assert.Equal(book.CategoryId, returnedBook.CategoryId);
        }


        [Fact]
        public async Task CreateBook_ReturnsCreated_WithValidDto()
        {
            // Arrange
            var bookCreateDto = new BookCreateDto { Title = "New Book", Author = "New Author", Price = 600000, Quantity = 50, CategoryId = "67eacc121af224f20dca6882" };
            var createdBook = new BookDto { BookId = "new-book-id", Title = "New Book", Author = "New Author", Price = 600000, Quantity = 50, CategoryId = "67eacc121af224f20dca6882" };
            _bookServiceMock.Setup(s => s.CreateBookAsync(bookCreateDto)).ReturnsAsync(createdBook);

            // Act
            var result = await _controller.CreateBook(bookCreateDto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal("GetBookById", createdResult.ActionName);
            var routeValues = createdResult.RouteValues;
            Assert.Equal(createdBook.BookId, routeValues["bookId"]);
            var returnedBook = Assert.IsType<BookDto>(createdResult.Value);
            Assert.Equal(createdBook.BookId, returnedBook.BookId);
            Assert.Equal(createdBook.Title, returnedBook.Title);
            Assert.Equal(createdBook.Author, returnedBook.Author);
            Assert.Equal(createdBook.Price, returnedBook.Price);
            Assert.Equal(createdBook.Quantity, returnedBook.Quantity);
            Assert.Equal(createdBook.CategoryId, returnedBook.CategoryId);
        }

        [Fact]
        public async Task CreateBook_ReturnsBadRequest_WhenModelStateInvalid()
        {
            // Arrange
            var bookCreateDto = new BookCreateDto();
            _controller.ModelState.AddModelError("Title", "The Title field is required.");

            // Act
            var result = await _controller.CreateBook(bookCreateDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var modelState = Assert.IsType<SerializableError>(badRequestResult.Value);
            Assert.True(modelState.ContainsKey("Title"));
            var errors = modelState["Title"] as string[];
            Assert.Contains("The Title field is required.", errors);
        }

        [Fact]
        public async Task UpdateBook_ReturnsOk_WithUpdatedBook()
        {
            // Arrange
            var bookId = "67eadd6a1af224f20dca684d";
            var bookUpdateDto = new BookUpdateDto { Title = "Updated Book", Author = "Updated Author", Price = 600000, Quantity = 60, CategoryId = "67eacc121af224f20dca6882" };
            var updatedBook = new BookDto { BookId = bookId, Title = "Updated Book", Author = "Updated Author", Price = 600000, Quantity = 60, CategoryId = "67eacc121af224f20dca6882" };
            _bookServiceMock.Setup(s => s.UpdateBookAsync(bookId, bookUpdateDto)).ReturnsAsync(updatedBook);

            // Act
            var result = await _controller.UpdateBook(bookId, bookUpdateDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedBook = Assert.IsType<BookDto>(okResult.Value);
            Assert.Equal(updatedBook.BookId, returnedBook.BookId);
            Assert.Equal(updatedBook.Title, returnedBook.Title);
            Assert.Equal(updatedBook.Author, returnedBook.Author);
            Assert.Equal(updatedBook.Price, returnedBook.Price);
            Assert.Equal(updatedBook.Quantity, returnedBook.Quantity);
            Assert.Equal(updatedBook.CategoryId, returnedBook.CategoryId);
        }

        [Fact]
        public async Task UpdateBook_ReturnsBadRequest_WhenModelStateInvalid()
        {
            // Arrange
            var bookId = "67eadd6a1af224f20dca684d";
            var bookUpdateDto = new BookUpdateDto();
            _controller.ModelState.AddModelError("Title", "The Title field is required.");

            // Act
            var result = await _controller.UpdateBook(bookId, bookUpdateDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var modelState = Assert.IsType<SerializableError>(badRequestResult.Value);
            Assert.True(modelState.ContainsKey("Title"));
            var errors = modelState["Title"] as string[];
            Assert.Contains("The Title field is required.", errors);
        }

        [Fact]
        public async Task DeleteBook_ReturnsNoContent_WhenBookExists()
        {
            // Arrange
            var bookId = "67eadd6a1af224f20dca684d";
            _bookServiceMock.Setup(s => s.DeleteBookAsync(bookId)).ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteBook(bookId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteBook_ReturnsNotFound_WhenBookDoesNotExist()
        {
            // Arrange
            var bookId = "non-existent-id";
            _bookServiceMock.Setup(s => s.DeleteBookAsync(bookId)).ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteBook(bookId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.NotNull(notFoundResult.Value);

            // Use reflection to access the 'message' property
            var valueType = notFoundResult.Value.GetType();
            var messageProperty = valueType.GetProperty("message");
            Assert.NotNull(messageProperty);

            var actualMessage = messageProperty.GetValue(notFoundResult.Value)?.ToString();
            var expectedMessage = $"Sách với ID {bookId} không tồn tại";
            Assert.Equal(expectedMessage, actualMessage);
        }
    }
}