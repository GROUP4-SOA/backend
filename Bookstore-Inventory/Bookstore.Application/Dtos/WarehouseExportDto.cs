using System;
using System.Collections.Generic;

namespace Bookstore.Application.Dtos
{
    public class WarehouseExportDto
    {
        public string ExportId { get; set; }
        public DateTime ExportDate { get; set; }
        public string UserId { get; set; }
        public List<WarehouseExportBookDto> WarehouseExportBooks { get; set; }
    }

    public class WarehouseExportBookDto
    {
        public string BookId { get; set; }
        public int ExportQuantity { get; set; } // Đồng bộ với WarehouseExportBook
    }
}