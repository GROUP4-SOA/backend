using Bookstore.Application.Dtos;

namespace Bookstore.Application.Interfaces.Services
{
    public interface ICategoryService
    {
        Task<List<CategoryDto>> GetAllCategoriesAsync();
        Task<CategoryDto> GetCategoryByIdAsync(string categoryid);
        Task<CategoryDto> CreateCategoryAsync(CategoryCreateDto createDto);
        Task<CategoryDto> UpdateCategoryAsync(string categoryid, CategoryUpdateDto updateDto);
        Task<bool> DeleteCategoryAsync(string categoryid);
    }
}