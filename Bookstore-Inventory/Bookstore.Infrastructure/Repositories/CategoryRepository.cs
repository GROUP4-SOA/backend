using Bookstore.Domain.Entities;
using Bookstore.Infrastructure.Data;
using Bookstore.Infrastructure.Interfaces.Repositories;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookstore.Infrastructure.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly IMongoCollection<Category> _categorys;
        public CategoryRepository(MongoDbContext dbContext)
        {
            _categorys = dbContext.GetCollection<Category>("Categorys");
        }
        public async Task<List<Category>> GetAllCategoriesAsync() =>
            await _categorys.Find(category => true).ToListAsync();


        public async Task<Category> GetCategoryByIdAsync(string id) =>
            await _categorys.Find(category => category.CategoryId == int.Parse(id)).FirstOrDefaultAsync();

        public async Task AddCategoryAsync(Category category) =>
            await _categorys.InsertOneAsync(category);

        public async Task UpdateCategoryAsync(Category category) =>
            await _categorys.ReplaceOneAsync(c => c.CategoryId == category.CategoryId, category);

        public async Task DeleteCategoryAsync(string id) =>
            await _categorys.DeleteOneAsync(category => category.CategoryId == int.Parse(id));
    }
}


