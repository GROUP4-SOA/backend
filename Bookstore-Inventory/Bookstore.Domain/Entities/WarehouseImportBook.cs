using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookstore.Domain.Entities
{
    public class WarehouseImportBook
    {
        public string WarehouseImportId { get; set; }
        public string BookId { get; set; }
        public int ImportQuantity { get; set; }
    }
}
