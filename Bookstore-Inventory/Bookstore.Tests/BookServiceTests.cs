using Bookstore.Application.Services;
using Bookstore.Infrastructure.Interfaces.Repositories;
using Bookstore.Domain.Entities;
using Bookstore.Application.Dtos;
using Moq;
using Xunit;
using System;
using System.Threading.Tasks;

public class BookServiceTests
{
    private readonly Mock<IBookRepository> _bookRepositoryMock;
    private readonly BookService _bookService;

    public BookServiceTests()
    {
        _bookRepositoryMock = new Mock<IBookRepository>();
        _bookService = new BookService(_bookRepositoryMock.Object);
    }

    [Fact]
    public async Task GetAllBooksAsync_ShouldReturnBooks()
    {
        // Giả lập dữ liệu dạng Book
        var fakeBooks = new List<Book>
        {
            new Book { BookId = "1", Title = "Test Book", Author = "John Doe", Price = 9.99m, CategoryId = "100" }
        };

        // Mock repository để trả về danh sách Book
        _bookRepositoryMock.Setup(repo => repo.GetAllBooksAsync()).ReturnsAsync(fakeBooks);

        // Gọi API
        var result = await _bookService.GetAllBooksAsync();

        // Kiểm tra kết quả
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Test Book", result[0].Title);
    }

}