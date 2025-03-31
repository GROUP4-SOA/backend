using Bookstore.Application.Interfaces;
using Bookstore.Domain.Entities;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bookstore.Infrastructure.Repositories
{
    public class WarehouseImportRepository : IWarehouseImportRepository
    {
        private readonly IMongoCollection<WarehouseImport> _collection;

        public WarehouseImportRepository(IMongoDatabase database)
        {
            _collection = database.GetCollection<WarehouseImport>("WarehouseImports");
        }

        public async Task<IEnumerable<WarehouseImport>> GetAllAsync()
            => await _collection.Find(_ => true).ToListAsync();

        public async Task<WarehouseImport> GetByIdAsync(int id)
            => await _collection.Find(x => x.ImportId == id).FirstOrDefaultAsync();

        public async Task AddAsync(WarehouseImport warehouseImport)
            => await _collection.InsertOneAsync(warehouseImport);

        public async Task UpdateAsync(WarehouseImport warehouseImport)
            => await _collection.ReplaceOneAsync(x => x.ImportId == warehouseImport.ImportId, warehouseImport);

        public async Task DeleteAsync(int id)
            => await _collection.DeleteOneAsync(x => x.ImportId == id);
    }
}
