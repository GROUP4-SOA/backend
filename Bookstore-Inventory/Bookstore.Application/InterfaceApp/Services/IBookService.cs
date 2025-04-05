using Bookstore.Application.Dtos;

namespace Bookstore.Application.Interfaces.Services
{
    public interface IBookService
    {
        Task<List<BookDto>> GetAllBooksAsync();
        Task<BookDto> GetBookByIdAsync(string bookId);
        Task<List<BookDto>> GetBooksByCategoryAsync(string categoryId);
        Task<BookDto> CreateBookAsync(BookCreateDto bookCreateDto);
        Task<BookDto> UpdateBookAsync(string bookId, BookUpdateDto bookUpdateDto);
        Task<bool> DeleteBookAsync(string bookId);
    }
}