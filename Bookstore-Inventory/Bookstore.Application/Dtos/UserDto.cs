using Bookstore.Domain.Entities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Bookstore.Application.Dtos
{
    public class UserDto
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; } = ObjectId.GenerateNewId().ToString();

        public string Username { get; set; } = null!;
        public string Password { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PhoneNo { get; set; } = null!;
        public UserRole Role { get; set; }
        public bool IsActive { get; set; }
    }
}
