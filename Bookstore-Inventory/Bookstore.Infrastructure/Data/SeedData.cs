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
                    new Book { BookId = 1, Title = "C# Programming", Author = "Author A", Price = 29.99M, Isbn = DateTime.Now, CategoryId = 1, Quantity = 10 },
                    new Book { BookId = 2, Title = "MongoDB Basics", Author = "Author B", Price = 24.99M, Isbn = DateTime.Now, CategoryId = 2, Quantity = 15 }
                };
                books.InsertMany(sampleBooks);
            }
        }
    }
}
