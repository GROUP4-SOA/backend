using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bookstore.Domain.Entities;
using Bookstore.Infrastructure.Data;
using Bookstore.Infrastructure.Interfaces.Repositories;
using MongoDB.Driver;

namespace Bookstore.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IMongoCollection<User> _users;
        public UserRepository(MongoDbContext dbContext)
        {
            _users = dbContext.GetCollection<User>("Users");
        }
        public async Task<List<User>> GetAllUsersAsync() =>
            await _users.Find(user => true).ToListAsync();


        public async Task<User> GetUserByIdAsync(string id) =>
            await _users.Find(user => user.UserId == id).FirstOrDefaultAsync();

        public async Task AddUserAsync(User user) =>
            await _users.InsertOneAsync(user);

        public async Task UpdateUserAsync(User user) =>
            await _users.ReplaceOneAsync(u => u.UserId == user.UserId, user);

        public async Task DeleteUserAsync(string id) =>
            await _users.DeleteOneAsync(user => user.UserId == id);
    }
}
