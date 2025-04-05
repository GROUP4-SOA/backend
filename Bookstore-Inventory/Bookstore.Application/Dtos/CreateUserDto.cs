using Bookstore.Domain.Entities;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Bookstore.Application.Dtos
{
    public class CreateUserDto
    {
        public string UserId { get; set; }=null!;
        [Required]
        public string Username { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;

        [Required]
        public string FullName { get; set; } = null!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [Phone]
        public string PhoneNo { get; set; } = null!;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public UserRole Role { get; set; } = UserRole.STAFF;

        public bool IsActive { get; set; } = true;
    }
}
