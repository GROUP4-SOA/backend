namespace Bookstore.Application.Dtos
{
    public class UserDto
    {
        public string UserId { get; set; }
        public string Username { get; set; } = null!;
        public string Role { get; set; } = null!;
    }
}