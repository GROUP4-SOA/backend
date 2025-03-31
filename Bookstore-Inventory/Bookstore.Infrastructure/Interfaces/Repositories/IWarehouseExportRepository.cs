using Bookstore.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bookstore.Application.Interfaces
{
    public interface IWarehouseExportRepository
    {
        Task<IEnumerable<WarehouseExport>> GetAllAsync();
        Task<WarehouseExport> GetByIdAsync(string id);
        Task AddAsync(WarehouseExport warehouseExport);
        Task UpdateAsync(WarehouseExport warehouseExport);
        Task DeleteAsync(string id);
    }
}
