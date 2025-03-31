using Bookstore.Domain.Entities;
using Bookstore.Infrastructure.Data;
using Bookstore.Infrastructure.Interfaces.Repositories;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bookstore.Infrastructure.Repositories
{
    public class BookRepository : IBookRepository
    {
        private readonly IMongoCollection<Book> _books;
        public BookRepository(MongoDbContext dbContext)
        {
            _books = dbContext.GetCollection<Book>("Books");
        }

        public async Task<List<Book>> GetAllBooksAsync() =>
            await _books.Find(book => true).ToListAsync();

        public async Task<Book> GetBookByIdAsync(string id) =>
            await _books.Find(book => book.BookId == id).FirstOrDefaultAsync();

        public async Task AddBookAsync(Book book) =>
            await _books.InsertOneAsync(book);

        public async Task UpdateBookAsync(Book book) =>
            await _books.ReplaceOneAsync(b => b.BookId == book.BookId, book);

        public async Task DeleteBookAsync(string id) =>
            await _books.DeleteOneAsync(book => book.BookId == id);
    }
}
