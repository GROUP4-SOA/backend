using Bookstore.Application.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bookstore.Application.Interfaces.Services
{
    public interface IExportService
    {
        Task<WarehouseExportDto> CreateExportAsync(WarehouseExportDto exportDto);
        Task<List<WarehouseExportDto>> GetAllExportsAsync();
    }
}