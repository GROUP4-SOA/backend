using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookstore.Domain.Entities
{
    public class WarehouseExport
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ExportId { get; set; } = ObjectId.GenerateNewId().ToString();

        public DateTime ExportDate { get; set; }
        public string UserId { get; set; }

        public List<WarehouseExportBook> WarehouseExportBooks { get; set; } = new();
    }
}
