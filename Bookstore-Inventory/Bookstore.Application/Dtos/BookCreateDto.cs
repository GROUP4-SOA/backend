namespace Bookstore.Application.Dtos
{
    public class BookCreateDto
    {
        public string BookId { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public decimal Price { get; set; }
        public string CategoryId { get; set; }
        public int Quantity { get; set; }
    }
}