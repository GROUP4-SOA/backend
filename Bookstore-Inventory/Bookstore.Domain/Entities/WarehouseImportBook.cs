using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Bookstore.Domain.Entities
{
    public class WarehouseImportBook
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string WarehouseImportId { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string ImportId { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string BookId { get; set; }
        public int ImportQuantity { get; set; }
        public decimal Price { get; set; }
    }
}
