using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookstore.Domain.Entities
{
    public class WarehouseExport
    {
        public int ExportId { get; set; }
        public DateTime ExportDate { get; set; }
        public List<Book> ExportedBooks { get; set; }
        public int ExportedById { get; set; }
        public User ExportedBy { get; set; }
    }
}
