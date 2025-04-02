using Bookstore.Application.Dtos;

namespace Bookstore.Application.Interfaces.Services
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync();
        Task<CategoryDto> GetCategoryByIdAsync(string id);
        Task<CategoryDto> CreateCategoryAsync(CategoryCreateDto createDto);
        Task<CategoryDto> UpdateCategoryAsync(string id, CategoryUpdateDto updateDto);
        Task<bool> DeleteCategoryAsync(string id);
    }
}