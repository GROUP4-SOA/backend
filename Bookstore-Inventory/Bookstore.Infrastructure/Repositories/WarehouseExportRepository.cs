using Bookstore.Application.Interfaces;
using Bookstore.Domain.Entities;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bookstore.Infrastructure.Repositories
{
    public class WarehouseExportRepository : IWarehouseExportRepository
    {
        private readonly IMongoCollection<WarehouseExport> _collection;

        public WarehouseExportRepository(IMongoDatabase database)
        {
            _collection = database.GetCollection<WarehouseExport>("WarehouseExports");
        }

        public async Task<IEnumerable<WarehouseExport>> GetAllAsync()
            => await _collection.Find(_ => true).ToListAsync();

        public async Task<WarehouseExport> GetByIdAsync(string id)
            => await _collection.Find(x => x.ExportId == id).FirstOrDefaultAsync();

        public async Task AddAsync(WarehouseExport warehouseExport)
            => await _collection.InsertOneAsync(warehouseExport);

        public async Task UpdateAsync(WarehouseExport warehouseExport)
            => await _collection.ReplaceOneAsync(x => x.ExportId == warehouseExport.ExportId, warehouseExport);

        public async Task DeleteAsync(string id)
            => await _collection.DeleteOneAsync(x => x.ExportId == id);
    }
}
