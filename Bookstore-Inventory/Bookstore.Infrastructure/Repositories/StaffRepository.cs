using Bookstore.Application.Interfaces;
using Bookstore.Domain.Entities;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bookstore.Infrastructure.Repositories
{
    public class StaffRepository : IStaffRepository
    {
        private readonly IMongoCollection<Staff> _collection;

        public StaffRepository(IMongoDatabase database)
        {
            _collection = database.GetCollection<Staff>("Staffs");
        }

        public async Task<IEnumerable<Staff>> GetAllAsync()
            => await _collection.Find(_ => true).ToListAsync();

        public async Task<Staff> GetByIdAsync(int id)
            => await _collection.Find(x => x.StaffId == id).FirstOrDefaultAsync();

        public async Task AddAsync(Staff staff)
            => await _collection.InsertOneAsync(staff);

        public async Task UpdateAsync(Staff staff)
            => await _collection.ReplaceOneAsync(x => x.StaffId == staff.StaffId, staff);

        public async Task DeleteAsync(int id)
            => await _collection.DeleteOneAsync(x => x.StaffId == id);
    }
}
