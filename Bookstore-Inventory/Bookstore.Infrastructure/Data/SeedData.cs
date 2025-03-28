//using Bookstore.Domain.Entities;
//using MongoDB.Driver;
//using System.Collections.Generic;

//namespace Bookstore.Infrastructure.Data
//{
//    public static class SeedData
//    {
//        public static void Initialize(MongoDbContext dbContext)
//        {
//            var books = dbContext.GetCollection<Book>("Books");

//            if (books.CountDocuments(_ => true) == 0)
//            {
//                var sampleBooks = new List<Book>
//                {
//                    new Book { Title = "C# Programming", Author = "Author A", Category = "Programming", Price = 29.99 },
//                    new Book { Title = "MongoDB Basics", Author = "Author B", Category = "Database", Price = 24.99 }
//                };
//                books.InsertMany(sampleBooks);
//            }
//        }
//    }
//}
