using Bookstore.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bookstore.Application.Interfaces
{
    public interface IWarehouseExportRepository
    {
        Task<IEnumerable<WarehouseExport>> GetAllAsync();
        Task<WarehouseExport> GetByIdAsync(int id);
        Task AddAsync(WarehouseExport warehouseExport);
        Task UpdateAsync(WarehouseExport warehouseExport);
        Task DeleteAsync(int id);
    }
}
