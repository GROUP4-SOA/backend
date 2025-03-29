// Bookstore.Application/Interfaces/Services/IBookService.cs
using Bookstore.Application.DTOs;

namespace Bookstore.Application.Interfaces.Services
{
    public interface IBookService
    {
        Task<List<BookDto>> GetAllBooksAsync();
        Task<BookDto> GetBookByIdAsync(string id);
        Task<BookDto> CreateBookAsync(BookCreateDto bookDto);
        Task UpdateBookAsync(string id, BookUpdateDto bookDto);
        Task DeleteBookAsync(string id);
    }
}