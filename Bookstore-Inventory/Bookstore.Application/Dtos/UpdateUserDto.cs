namespace Bookstore.Application.Dtos
{
    public class UpdateUserDto
    {
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PhoneNo { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}