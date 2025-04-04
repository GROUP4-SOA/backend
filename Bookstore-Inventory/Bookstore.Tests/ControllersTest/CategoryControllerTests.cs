using Bookstore.Application.Dtos;
using Bookstore.Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using MongoDB.Driver;
using Moq;
using Xunit;

namespace Bookstore.Tests.Controllers
{
    public class CategoryControllerTests
    {
        private readonly Mock<CategoryService> _categoryServiceMock;

        public CategoryControllerTests()
        {
            _categoryServiceMock = new Mock<CategoryService>(Mock.Of<IMongoDatabase>());
        }

        [Fact]
        public async Task GetAllCategories_ReturnsOk_WithCategories()
        {
            // Arrange
            var categories = new List<CategoryDto> { new() { CategoryId = "cat1", Name = "Fiction", Description = "Fiction books" } };
            _categoryServiceMock.Setup(s => s.GetAllCategoriesAsync()).ReturnsAsync(categories);
            Func<CategoryService, Task<IResult>> getAllCategories = async (service) => Results.Ok(await service.GetAllCategoriesAsync());

            // Act
            var result = await getAllCategories(_categoryServiceMock.Object);

            // Assert
            var okResult = Assert.IsType<Ok<List<CategoryDto>>>(result);
            Assert.Equal(categories, okResult.Value);
        }

        [Fact]
        public async Task GetCategoryById_ReturnsOk_WithCategory()
        {
            // Arrange
            var category = new CategoryDto { CategoryId = "cat1", Name = "Fiction", Description = "Fiction books" };
            _categoryServiceMock.Setup(s => s.GetCategoryByIdAsync("cat1")).ReturnsAsync(category);
            Func<string, CategoryService, Task<IResult>> getCategoryById = async (categoryId, service) => Results.Ok(await service.GetCategoryByIdAsync(categoryId));

            // Act
            var result = await getCategoryById("cat1", _categoryServiceMock.Object);

            // Assert
            var okResult = Assert.IsType<Ok<CategoryDto>>(result);
            Assert.Equal(category, okResult.Value);
        }

        [Fact]
        public async Task CreateCategory_ReturnsCreated_WithValidDto()
        {
            // Arrange
            var dto = new CategoryCreateDto { Name = "Fiction", Description = "Fiction books" };
            var createdCategory = new CategoryDto { CategoryId = "cat1", Name = "Fiction", Description = "Fiction books" };
            _categoryServiceMock.Setup(s => s.CreateCategoryAsync(dto)).ReturnsAsync(createdCategory);
            Func<CategoryCreateDto, CategoryService, Task<IResult>> createCategory = async (dto, service) =>
            {
                var created = await service.CreateCategoryAsync(dto);
                return Results.Created($"/api/categories/{created.CategoryId}", created);
            };

            // Act
            var result = await createCategory(dto, _categoryServiceMock.Object);

            // Assert
            var createdResult = Assert.IsType<Created<CategoryDto>>(result);
            Assert.Equal($"/api/categories/{createdCategory.CategoryId}", createdResult.Location);
            Assert.Equal(createdCategory, createdResult.Value);
        }

        [Fact]
        public async Task UpdateCategory_ReturnsOk_WithValidInput()
        {
            // Arrange
            var dto = new CategoryUpdateDto { Name = "Updated Fiction", Description = "Updated desc" };
            var updatedCategory = new CategoryDto { CategoryId = "cat1", Name = "Updated Fiction", Description = "Updated desc" };
            _categoryServiceMock.Setup(s => s.UpdateCategoryAsync("cat1", dto)).ReturnsAsync(updatedCategory);
            Func<string, CategoryUpdateDto, CategoryService, Task<IResult>> updateCategory = async (categoryId, dto, service) => Results.Ok(await service.UpdateCategoryAsync(categoryId, dto));

            // Act
            var result = await updateCategory("cat1", dto, _categoryServiceMock.Object);

            // Assert
            var okResult = Assert.IsType<Ok<CategoryDto>>(result);
            Assert.Equal(updatedCategory, okResult.Value);
        }

        [Fact]
        public async Task DeleteCategory_ReturnsNoContent_WhenSuccessful()
        {
            // Arrange
            _categoryServiceMock.Setup(s => s.DeleteCategoryAsync("cat1")).ReturnsAsync(true);
            Func<string, CategoryService, Task<IResult>> deleteCategory = async (categoryId, service) =>
                await service.DeleteCategoryAsync(categoryId) ? Results.NoContent() : Results.NotFound();

            // Act
            var result = await deleteCategory("cat1", _categoryServiceMock.Object);

            // Assert
            Assert.IsType<NoContent>(result);
        }
    }
}