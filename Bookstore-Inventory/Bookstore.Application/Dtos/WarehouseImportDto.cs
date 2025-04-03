using System;
using System.Collections.Generic;

namespace Bookstore.Application.Dtos
{
    public class WarehouseImportDto
    {
        public string ImportId { get; set; }
        public DateTime ImportDate { get; set; }
        public string UserId { get; set; }
        public List<WarehouseImportBookDto> WarehouseImportBooks { get; set; }
    }

    public class WarehouseImportBookDto
    {
        public string BookId { get; set; }
        public int ImportQuantity { get; set; }
        public decimal Price { get; set; }
    }
}