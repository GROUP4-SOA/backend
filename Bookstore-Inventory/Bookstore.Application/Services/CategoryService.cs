using Bookstore.Application.Dtos;
using Bookstore.Application.Interfaces.Services;
using Bookstore.Domain.Entities;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace Bookstore.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IMongoCollection<Category> _categoriesCollection;

        public CategoryService(IMongoDatabase database)
        {
            _categoriesCollection = database.GetCollection<Category>("Category");
        }

        public async Task<List<CategoryDto>> GetAllCategoriesAsync()
        {
            var categories = await _categoriesCollection.Find(_ => true).ToListAsync();
            return categories.Select(c => new CategoryDto
            {
                CategoryId = c.Id,
                Name = c.Name,
                Description = c.Description
            }).ToList();
        }

        public async Task<CategoryDto> GetCategoryByIdAsync(string categoryId)
        {
            if (string.IsNullOrEmpty(categoryId))
                throw new ArgumentException("CategoryId không được để trống");

            var category = await _categoriesCollection.Find(c => c.Id == categoryId).FirstOrDefaultAsync();
            if (category == null)
                throw new ArgumentException($"Danh mục với ID {categoryId} không tồn tại");

            return new CategoryDto
            {
                CategoryId = category.Id,
                Name = category.Name,
                Description = category.Description
            };
        }

        public async Task<CategoryDto> CreateCategoryAsync(CategoryCreateDto categoryCreateDto)
        {
            // Không cần kiểm tra Id trùng lặp vì Id sẽ được tạo tự động
            var category = new Category
            {
                // Id sẽ được tạo tự động bởi MongoDB
                Name = categoryCreateDto.Name,
                Description = categoryCreateDto.Description
            };

            await _categoriesCollection.InsertOneAsync(category);

            return new CategoryDto
            {
                CategoryId = category.Id,
                Name = category.Name,
                Description = category.Description
            };
        }

        public async Task<CategoryDto> UpdateCategoryAsync(string categoryId, CategoryUpdateDto categoryUpdateDto)
        {
            if (string.IsNullOrEmpty(categoryId))
                throw new ArgumentException("CategoryId không được để trống");

            var category = await _categoriesCollection.Find(c => c.Id == categoryId).FirstOrDefaultAsync();
            if (category == null)
                throw new ArgumentException($"Danh mục với ID {categoryId} không tồn tại");

            // Không thay đổi Id vì đó là khóa chính
            category.Name = categoryUpdateDto.Name;
            category.Description = categoryUpdateDto.Description;

            var filter = Builders<Category>.Filter.Eq(c => c.Id, categoryId);
            await _categoriesCollection.ReplaceOneAsync(filter, category);

            return new CategoryDto
            {
                CategoryId = category.Id,
                Name = category.Name,
                Description = category.Description
            };
        }

        public async Task<bool> DeleteCategoryAsync(string categoryId)
        {
            if (string.IsNullOrEmpty(categoryId))
                throw new ArgumentException("CategoryId không được để trống");

            var filter = Builders<Category>.Filter.Eq(c => c.Id, categoryId);
            var result = await _categoriesCollection.DeleteOneAsync(filter);
            return result.DeletedCount > 0;
        }
    }
}