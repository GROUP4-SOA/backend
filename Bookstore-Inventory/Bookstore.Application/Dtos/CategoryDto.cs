namespace Bookstore.Application.Dtos
{
    public class CategoryDto
    {
        public string CategoryId { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
    }
}