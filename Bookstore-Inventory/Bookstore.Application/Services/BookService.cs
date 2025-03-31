// Bookstore.Application/Services/BookService.cs
using Bookstore.Application.DTOs;
using Bookstore.Application.Interfaces.Services;
using Bookstore.Infrastructure.Interfaces.Repositories;
using Bookstore.Domain.Entities;

namespace Bookstore.Application.Services
{
    public class BookService : IBookService
    {
        private readonly IBookRepository _bookRepository;

        public BookService(IBookRepository bookRepository)
        {
            _bookRepository = bookRepository;
        }

        public async Task<List<BookDto>> GetAllBooksAsync()
        {
            var books = await _bookRepository.GetAllAsync();
            return books.Select(b => new BookDto
            {
                Id = b.Id,
                Title = b.Title,
                Price = b.Price,
                CategoryId = b.CategoryId
            }).ToList();
        }

        public async Task<BookDto> GetBookByIdAsync(string id)
        {
            var book = await _bookRepository.GetByIdAsync(id);
            if (book == null) return null;
            return new BookDto
            {
                Id = book.Id,
                Title = book.Title,
                Price = book.Price,
                CategoryId = book.CategoryId
            };
        }

        public async Task<BookDto> CreateBookAsync(BookCreateDto bookDto)
        {
            var book = new Book
            {
                Id = Guid.NewGuid().ToString(),
                Title = bookDto.Title,
                Price = bookDto.Price,
                CategoryId = bookDto.CategoryId
            };
            await _bookRepository.AddAsync(book);
            return new BookDto
            {
                Id = book.Id,
                Title = book.Title,
                Price = book.Price,
                CategoryId = book.CategoryId
            };
        }

        public async Task UpdateBookAsync(string id, BookUpdateDto bookDto)
        {
            var book = await _bookRepository.GetByIdAsync(id);
            if (book == null) throw new Exception("Book not found");

            book.Title = bookDto.Title;
            book.Price = bookDto.Price;
            book.CategoryId = bookDto.CategoryId;
            await _bookRepository.UpdateAsync(book);
        }

        public async Task DeleteBookAsync(string id)
        {
            var book = await _bookRepository.GetByIdAsync(id);
            if (book == null) throw new Exception("Book not found");
            await _bookRepository.DeleteAsync(id);
        }
    }
}