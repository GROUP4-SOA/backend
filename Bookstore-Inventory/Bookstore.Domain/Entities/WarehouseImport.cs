using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookstore.Domain.Entities
{
    public class WarehouseImport
    {
        public int ImportId { get; set; }
        public DateTime ImportDate { get; set; }
        public List<Book> ImportedBooks { get; set; }
        public int ImportedById { get; set; }
        public User ImportedBy { get; set; }
    }
}
