using Bookstore.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bookstore.Application.Interfaces
{
    public interface IAdminRepository
    {
        Task<IEnumerable<Admin>> GetAllAsync();
        Task<Admin> GetByIdAsync(string id);
        Task AddAsync(Admin admin);
        Task UpdateAsync(Admin admin);
        Task DeleteAsync(string id);
    }
}
