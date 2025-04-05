using Bookstore.Application.Dtos;
using Bookstore.Application.Interfaces.Services;
using Bookstore.Domain.Entities;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Bookstore.Application.Services
{
    public class BookService : IBookService
    {
        private readonly IMongoCollection<Book> _booksCollection;

        public BookService(IMongoDatabase database)
        {
            _booksCollection = database.GetCollection<Book>("Book");
        }

        public async Task<List<BookDto>> GetAllBooksAsync()
        {
            var books = await _booksCollection.Find(_ => true).ToListAsync();
            return books.Select(b => new BookDto
            {
                BookId = b.BookId,
                Title = b.Title,
                Author = b.Author,
                Price = b.Price,
                CategoryId = b.CategoryId,
                Quantity = b.Quantity
            }).ToList();
        }

        public async Task<BookDto> GetBookByIdAsync(string bookId)
        {
            if (string.IsNullOrEmpty(bookId))
                throw new ArgumentException("BookId không được để trống");

            var book = await _booksCollection.Find(b => b.BookId == bookId).FirstOrDefaultAsync();
            if (book == null)
                throw new ArgumentException($"Sách với ID {bookId} không tồn tại");

            return new BookDto
            {
                BookId = book.BookId,
                Title = book.Title,
                Author = book.Author,
                Price = book.Price,
                CategoryId = book.CategoryId,
                Quantity = book.Quantity
            };
        }
        public async Task<List<BookDto>> GetBooksByCategoryAsync(string categoryId)
        {
            if (string.IsNullOrEmpty(categoryId))
                throw new ArgumentException("CategoryId không được để trống");

            // Log để kiểm tra categoryId
            Console.WriteLine($"Fetching books for category: {categoryId}");

            // Lấy danh sách sách thuộc categoryId từ cơ sở dữ liệu
            var book = await _booksCollection
                .Find(book => book.CategoryId == categoryId)
                .ToListAsync();

            return book.Select(book => new BookDto
            {
                BookId = book.BookId,
                Title = book.Title,
                Author = book.Author,
                Price = book.Price,
                Quantity = book.Quantity,
                CategoryId = book.CategoryId
            }).ToList();
        }


        public async Task<BookDto> CreateBookAsync(BookCreateDto bookCreateDto)
        {
            var existingBook = await _booksCollection.Find(b => b.BookId == bookCreateDto.BookId).FirstOrDefaultAsync();
            if (existingBook != null)
                throw new ArgumentException($"Sách với BookId {bookCreateDto.BookId} đã tồn tại");

            var book = new Book
            {
                BookId = bookCreateDto.BookId,
                Title = bookCreateDto.Title,
                Author = bookCreateDto.Author,
                Price = bookCreateDto.Price,
                CategoryId = bookCreateDto.CategoryId,
                Quantity = bookCreateDto.Quantity
            };

            await _booksCollection.InsertOneAsync(book);

            return new BookDto
            {
                BookId = book.BookId,
                Title = book.Title,
                Author = book.Author,
                Price = book.Price,
                CategoryId = book.CategoryId,
                Quantity = book.Quantity
            };
        }

        public async Task<BookDto> UpdateBookAsync(string bookId, BookUpdateDto bookUpdateDto)
        {
            if (string.IsNullOrEmpty(bookId))
                throw new ArgumentException("BookId không được để trống");

            var book = await _booksCollection.Find(b => b.BookId == bookId).FirstOrDefaultAsync();
            if (book == null)
                throw new ArgumentException($"Sách với ID {bookId} không tồn tại");

            book.Title = bookUpdateDto.Title;
            book.Author = bookUpdateDto.Author;
            book.Price = bookUpdateDto.Price;
            book.CategoryId = bookUpdateDto.CategoryId;
            book.Quantity = bookUpdateDto.Quantity;

            await _booksCollection.ReplaceOneAsync(b => b.BookId == bookId, book);

            return new BookDto
            {
                BookId = book.BookId,
                Title = book.Title,
                Author = book.Author,
                Price = book.Price,
                CategoryId = book.CategoryId,
                Quantity = book.Quantity
            };
        }

        public async Task<bool> DeleteBookAsync(string bookId)
        {
            if (string.IsNullOrEmpty(bookId))
                throw new ArgumentException("BookId không được để trống");

            var result = await _booksCollection.DeleteOneAsync(b => b.BookId == bookId);
            return result.DeletedCount > 0;
        }
    }
}