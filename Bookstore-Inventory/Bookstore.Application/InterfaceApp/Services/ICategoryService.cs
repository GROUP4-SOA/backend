using Bookstore.Application.Dtos;

namespace Bookstore.Application.Interfaces.Services
{
    public interface ICategoryService
    {
        Task<List<CategoryDto>> GetAllCategoriesAsync();
        Task<CategoryDto> GetCategoryByIdAsync(string categoryId);
        Task<CategoryDto> CreateCategoryAsync(CategoryDto categoryDto);
        Task<CategoryDto> UpdateCategoryAsync(string categoryId, CategoryDto categoryDto);
        Task<bool> DeleteCategoryAsync(string categoryId);
    }
}