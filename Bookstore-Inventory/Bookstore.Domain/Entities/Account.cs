using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookstore.Domain.Entities
{
    public class Account
    {
        public int AccountId { get; set; }
        public UserRole Role { get; set; }
        public string Password { get; set; }
        public bool IsActive { get; set; }
    }
}
