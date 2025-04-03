using Bookstore.Domain.Entities;
using Bookstore.Infrastructure.Data;
using Bookstore.Infrastructure.Interfaces.Repositories;
using MongoDB.Driver;

namespace Bookstore.Infrastructure.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly IMongoCollection<Category> _categories;

        public CategoryRepository(MongoDbContext dbContext)
        {
            _categories = dbContext.GetCollection<Category>("Category");
        }

        public async Task<List<Category>> GetAllCategoriesAsync() =>
            await _categories.Find(category => true).ToListAsync();

        public async Task<Category> GetCategoryByIdAsync(string id) =>
            await _categories.Find(category => category.CategoryId == id).FirstOrDefaultAsync();

        public async Task AddCategoryAsync(Category category) =>
            await _categories.InsertOneAsync(category);

        public async Task UpdateCategoryAsync(Category category) =>
            await _categories.ReplaceOneAsync(c => c.CategoryId == category.CategoryId, category);

        public async Task DeleteCategoryAsync(string id) =>
            await _categories.DeleteOneAsync(category => category.CategoryId == id);
    }
}