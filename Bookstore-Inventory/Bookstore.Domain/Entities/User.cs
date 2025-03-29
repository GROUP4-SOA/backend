using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace Bookstore.Domain.Entities
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; } = ObjectId.GenerateNewId().ToString();

        public string Username { get; set; }
        public string Email { get; set; }
        public string PhoneNo { get; set; }

        // Liên kết với Account bằng ObjectId thay vì lưu trực tiếp đối tượng Account
        [BsonRepresentation(BsonType.ObjectId)]
        public string? AccountId { get; set; }
        [BsonRepresentation(BsonType.String)]
        public UserRole Role { get; set; }
    }
}
