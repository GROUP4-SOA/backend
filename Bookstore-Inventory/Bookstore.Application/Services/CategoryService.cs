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
            _categoriesCollection = database.GetCollection<Category>("Categories");
        }

        public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
        {
            var categories = await _categoriesCollection.Find(_ => true).ToListAsync();
            return categories.Select(c => new CategoryDto
            {
                CategoryId = c.Id,
                Name = c.Name,
                Description = c.Description
            });
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
            var category = new Category
            {
                Id = ObjectId.GenerateNewId().ToString(),
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

            var category = await _categoriesCollection
                .Find(c => c.Id == categoryId).FirstOrDefaultAsync();
            if (category == null)
                throw new ArgumentException($"Danh mục với ID {categoryId} không tồn tại");

            category.Name = categoryUpdateDto.Name;
            category.Description = categoryUpdateDto.Description;

            await _categoriesCollection.ReplaceOneAsync(c => c.Id == categoryId, category);

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

            var result = await _categoriesCollection.DeleteOneAsync(c => c.Id == categoryId);
            return result.DeletedCount > 0;
        }
    }
}