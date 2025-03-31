using Bookstore.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bookstore.Application.Interfaces
{
    public interface IStaffRepository
    {
        Task<IEnumerable<Staff>> GetAllAsync();
        Task<Staff> GetByIdAsync(string id);
        Task AddAsync(Staff staff);
        Task UpdateAsync(Staff staff);
        Task DeleteAsync(string id);
    }
}
