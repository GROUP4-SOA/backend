using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookstore.Domain.Entities
{
    public class Account
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string AccountId { get; set; } = ObjectId.GenerateNewId().ToString();
        [BsonRepresentation(BsonType.String)]
        public UserRole Role { get; set; }
        public string Password { get; set; }
        public bool IsActive { get; set; }

        // Không cần ForeignKey, thay bằng ObjectId UserId
        public string UserId { get; set; }
    }
}
