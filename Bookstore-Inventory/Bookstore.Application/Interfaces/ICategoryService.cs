// Bookstore.Application/Interfaces/Services/ICategoryService.cs
using Bookstore.Application.DTOs;

namespace Bookstore.Application.Interfaces.Services
{
    public interface ICategoryService
    {
        Task<List<CategoryDto>> GetAllCategoriesAsync();
        Task<CategoryDto> GetCategoryByIdAsync(string id);
        Task<CategoryDto> CreateCategoryAsync(CategoryDto categoryDto);
        Task UpdateCategoryAsync(string id, CategoryDto categoryDto);
        Task DeleteCategoryAsync(string id);
    }
}