using Bookstore.Application.Interfaces;
using Bookstore.Domain.Entities;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bookstore.Infrastructure.Repositories
{
    public class AdminRepository : IAdminRepository
    {
        private readonly IMongoCollection<Admin> _collection;

        public AdminRepository(IMongoDatabase database)
        {
            _collection = database.GetCollection<Admin>("Admin");
        }

        public async Task<IEnumerable<Admin>> GetAllAsync()
            => await _collection.Find(_ => true).ToListAsync();

        public async Task<Admin> GetByIdAsync(string id)
            => await _collection.Find(x => x.AdminId == id).FirstOrDefaultAsync();

        public async Task AddAsync(Admin admin)
            => await _collection.InsertOneAsync(admin);

        public async Task UpdateAsync(Admin admin)
            => await _collection.ReplaceOneAsync(x => x.AdminId == admin.AdminId, admin);

        public async Task DeleteAsync(string id)
            => await _collection.DeleteOneAsync(x => x.AdminId == id);
    }
}
