using Bookstore.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bookstore.Application.Interfaces
{
    public interface IWarehouseImportRepository
    {
        Task<IEnumerable<WarehouseImport>> GetAllAsync();
        Task<WarehouseImport> GetByIdAsync(string id);
        Task AddAsync(WarehouseImport warehouseImport);
        Task UpdateAsync(WarehouseImport warehouseImport);
        Task DeleteAsync(string id);
    }
}
