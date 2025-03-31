using Bookstore.Application.Dtos;
using Bookstore.Application.Interfaces.Services;
using Bookstore.Domain.Entities;
using Bookstore.Infrastructure.Interfaces;
using Bookstore.Infrastructure.Interfaces.Repositories;

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
            var categories = await _categoryRepository.GetAllCategoriesAsync();
            var categoryDtos = new List<CategoryDto>();

            foreach (var category in categories)
            {
                categoryDtos.Add(new CategoryDto
                {
                    CategoryId = category.CategoryId,
                    Name = category.Name,
                    Description = category.Description
                });
            }

            return categoryDtos;
        }

        public async Task<CategoryDto> GetCategoryByIdAsync(string categoryId)
        {
            var category = await _categoryRepository.GetCategoryByIdAsync(categoryId);
            if (category == null)
            {
                return null;
            }

            return new CategoryDto
            {
                CategoryId = category.CategoryId,
                Name = category.Name,
                Description = category.Description
            };
        }

        public async Task<CategoryDto> CreateCategoryAsync(CategoryDto categoryDto)
        {
            var category = new Category
            {
                CategoryId = "ABC",
                Name = categoryDto.Name,
                Description = categoryDto.Description
            };

            await _categoryRepository.AddCategoryAsync(category);

            return new CategoryDto
            {
                CategoryId = category.CategoryId,
                Name = category.Name,
                Description = category.Description
            };
        }

        public async Task<CategoryDto> UpdateCategoryAsync(string categoryId, CategoryDto categoryDto)
        {
            var existingCategory = await _categoryRepository.GetCategoryByIdAsync(categoryId);
            if (existingCategory == null)
            {
                return null;
            }

            existingCategory.Name = categoryDto.Name;
            existingCategory.Description = categoryDto.Description;

            await _categoryRepository.UpdateCategoryAsync(existingCategory);

            return new CategoryDto
            {
                CategoryId = existingCategory.CategoryId,
                Name = existingCategory.Name,
                Description = existingCategory.Description
            };
        }

        public async Task<bool> DeleteCategoryAsync(string categoryId)
        {
            var category = await _categoryRepository.GetCategoryByIdAsync(categoryId);
            if (category == null)
            {
                return false;
            }

            await _categoryRepository.DeleteCategoryAsync(categoryId);
            return true;
        }
    }
}