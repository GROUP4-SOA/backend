using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookstore.Domain.Entities
{
    public class WarehouseImport
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ImportId { get; set; } = ObjectId.GenerateNewId().ToString();

        public DateTime ImportDate { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; }

        public List<WarehouseImportBook> WarehouseImportBooks { get; set; } = new();
    }
}
