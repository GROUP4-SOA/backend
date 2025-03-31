using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookstore.Domain.Entities
{
    public class WarehouseExportBook
    {
        public string WarehouseExportId { get; set; }
        public string BookId { get; set; }
        public int ExportQuantity { get; set; }
    }
}
