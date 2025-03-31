using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookstore.Domain.Entities
{
    public class Book
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public int BookId { get; set; }

        public string Title { get; set; }
        public string Author { get; set; }
        public decimal Price { get; set; }

        public string ISBN { get; set; }

        public string CategoryId { get; set; }
        public int Quantity { get; set; }
    }
}
