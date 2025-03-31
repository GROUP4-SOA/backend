using Bookstore.Application.Dtos;

namespace Bookstore.Application.Interfaces.Services
{
    public interface ICategoryService
    {
        Task<List<CategoryDto>> GetAllCategoriesAsync();
        Task<CategoryDto> GetCategoryByIdAsync(int categoryId);
        Task<CategoryDto> CreateCategoryAsync(CategoryDto categoryDto);
        Task<CategoryDto> UpdateCategoryAsync(int categoryId, CategoryDto categoryDto);
        Task<bool> DeleteCategoryAsync(int categoryId);
    }
}