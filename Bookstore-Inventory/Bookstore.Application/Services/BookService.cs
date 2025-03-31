using Bookstore.Application.Dtos;
using Bookstore.Application.Interfaces.Services;
using Bookstore.Domain.Entities;
using Bookstore.Infrastructure.Interfaces;
using Bookstore.Infrastructure.Interfaces.Repositories;

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
            var books = await _bookRepository.GetAllBooksAsync();
            var bookDtos = new List<BookDto>();

            foreach (var book in books)
            {
                bookDtos.Add(new BookDto
                {
                    BookId = book.BookId,
                    Title = book.Title,
                    Author = book.Author,
                    Price = book.Price,
                    CategoryId = book.CategoryId
                });
            }

            return bookDtos;
        }

        public async Task<BookDto> GetBookByIdAsync(string bookId)
        {
            var book = await _bookRepository.GetBookByIdAsync(bookId.ToString());
            if (book == null)
            {
                return null;
            }

            return new BookDto
            {
                BookId = book.BookId,
                Title = book.Title,
                Author = book.Author,
                Price = book.Price,
                CategoryId = book.CategoryId
            };
        }

        public async Task<BookDto> CreateBookAsync(BookCreateDto bookCreateDto)
        {
            var book = new Book
            {
                BookId = "ABC",
                Title = bookCreateDto.Title,
                Author = bookCreateDto.Author,
                Price = bookCreateDto.Price,
                CategoryId = bookCreateDto.CategoryId
            };

            await _bookRepository.AddBookAsync(book);

            return new BookDto
            {
                BookId = book.BookId,
                Title = book.Title,
                Author = book.Author,
                Price = book.Price,
                CategoryId = book.CategoryId
            };
        }

        public async Task<BookDto> UpdateBookAsync(string bookId, BookUpdateDto bookUpdateDto)
        {
            var existingBook = await _bookRepository.GetBookByIdAsync(bookId.ToString());
            if (existingBook == null)
            {
                return null;
            }

            existingBook.Title = bookUpdateDto.Title;
            existingBook.Author = bookUpdateDto.Author;
            existingBook.Price = bookUpdateDto.Price;
            existingBook.CategoryId = bookUpdateDto.CategoryId;

            await _bookRepository.UpdateBookAsync(existingBook);

            return new BookDto
            {
                BookId = existingBook.BookId,
                Title = existingBook.Title,
                Author = existingBook.Author,
                Price = existingBook.Price,
                CategoryId = existingBook.CategoryId
            };
        }

        public async Task<bool> DeleteBookAsync(string bookId)
        {
            var book = await _bookRepository.GetBookByIdAsync(bookId.ToString());
            if (book == null)
            {
                return false;
            }

            await _bookRepository.DeleteBookAsync(bookId.ToString());
            return true;
        }
    }
}