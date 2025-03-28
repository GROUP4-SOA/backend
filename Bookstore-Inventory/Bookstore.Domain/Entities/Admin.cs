using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookstore.Domain.Entities
{
    public class Admin
    {
        public int AdminId { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
    }
}
