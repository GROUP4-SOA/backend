// Bookstore.Application/Services/CategoryService.cs
using Bookstore.Application.DTOs;
using Bookstore.Application.Interfaces.Services;
using Bookstore.Infrastructure.Interfaces.Repositories;
using Bookstore.Domain.Entities;

namespace Bookstore.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<List<CategoryDto>> GetAllCategoriesAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();
            return categories.Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name
            }).ToList();
        }

        public async Task<CategoryDto> GetCategoryByIdAsync(string id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null) return null;
            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name
            };
        }

        public async Task<CategoryDto> CreateCategoryAsync(CategoryDto categoryDto)
        {
            var category = new Category
            {
                Id = Guid.NewGuid().ToString(),
                Name = categoryDto.Name
            };
            await _categoryRepository.AddAsync(category);
            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name
            };
        }

        public async Task UpdateCategoryAsync(string id, CategoryDto categoryDto)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null) throw new Exception("Category not found");

            category.Name = categoryDto.Name;
            await _categoryRepository.UpdateAsync(category);
        }

        public async Task DeleteCategoryAsync(string id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null) throw new Exception("Category not found");
            await _categoryRepository.DeleteAsync(id);
        }
    }
}