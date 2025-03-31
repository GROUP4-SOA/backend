﻿namespace Bookstore.Application.Dtos
{
    public class BookCreateDto
    {
        public string Title { get; set; } = null!;
        public string Author { get; set; } = null!;
        public decimal Price { get; set; }
        public string CategoryId { get; set; }
    }
}