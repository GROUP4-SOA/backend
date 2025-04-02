namespace Bookstore.Application.Dtos
{
    public class CategoryDto
    {
        public string CategoryId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class CategoryCreateDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class CategoryUpdateDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}