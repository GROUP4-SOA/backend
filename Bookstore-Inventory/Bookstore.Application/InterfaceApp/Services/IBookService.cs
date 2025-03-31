using Bookstore.Application.Dtos;

namespace Bookstore.Application.Interfaces.Services
{
    public interface IBookService
    {
        Task<List<BookDto>> GetAllBooksAsync();
        Task<BookDto> GetBookByIdAsync(int bookId);
        Task<BookDto> CreateBookAsync(BookCreateDto bookCreateDto);
        Task<BookDto> UpdateBookAsync(int bookId, BookUpdateDto bookUpdateDto);
        Task<bool> DeleteBookAsync(int bookId);
    }
}