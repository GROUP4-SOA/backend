using Bookstore.Domain.Entities;

namespace Bookstore.Application.Dtos
{
    public class RegisterRequestDto
    {
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PhoneNo { get; set; } = null!;
        public UserRole Role { get; set; }
    }
}