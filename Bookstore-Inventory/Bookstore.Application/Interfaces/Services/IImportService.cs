using Bookstore.Application.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bookstore.Application.Interfaces.Services
{
    public interface IImportService
    {
        Task<WarehouseImportDto> CreateImportAsync(WarehouseImportDto importDto);
        Task<List<WarehouseImportDto>> GetAllImportsAsync();
    }
}