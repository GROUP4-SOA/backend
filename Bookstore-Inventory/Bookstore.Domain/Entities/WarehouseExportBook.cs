using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Bookstore.Domain.Entities
{
    public class WarehouseExportBook
    {
     
        [BsonRepresentation(BsonType.ObjectId)]
        public string WarehouseExportId { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string BookId { get; set; } = null!; 
        public int ExportQuantity { get; set; }
        public decimal Price { get; set; }
    }
}
