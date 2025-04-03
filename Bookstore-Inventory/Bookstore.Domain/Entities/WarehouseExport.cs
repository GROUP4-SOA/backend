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
        [BsonElement("ExportDate")]
        public DateTime ExportDate { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; }
        [BsonElement("WarehouseExportBooks")]
        public List<WarehouseExportBook> WarehouseExportBooks { get; set; } = new();
    }
}
