using Bookstore.Domain.Entities;
using System.Text.Json.Serialization;

namespace Bookstore.Application.Dtos
{
    public class UpdateUserDto
    {
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PhoneNo { get; set; } = null!;
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public UserRole Role { get; set; }

        public bool? IsActive { get; set; } = null; // Nullable bool
        public string Username { get; set; } = null!;
    }
}
