using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookstore.Domain.Entities
{
    public class Staff
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string StaffId { get; set; } = ObjectId.GenerateNewId().ToString();
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; } = null!;
    }
}
