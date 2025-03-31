using System;
using System.Collections.Generic;
using Bookstore.Domain.Entities;
using MongoDB.Driver;

namespace Bookstore.Infrastructure.Data
{
    public static class SeedData
    {
        public static void Initialize(MongoDbContext dbContext)
        {
            var books = dbContext.GetCollection<Book>("Books");

            if (books.CountDocuments(_ => true) == 0)
            {
                var sampleBooks = new List<Book>
                {
                    new Book { BookId = "A1B", Title = "C# Programming", Author = "Author A", Price = 29.99M, ISBN = "1A2B3C", CategoryId = "1A", Quantity = 10 },
                    new Book { BookId = "32C", Title = "MongoDB Basics", Author = "Author B", Price = 24.99M, ISBN = "4A5B6C", CategoryId = "2B", Quantity = 15 }
                };
                books.InsertMany(sampleBooks);
            }
        }
    }
}
