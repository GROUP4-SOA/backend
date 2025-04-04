using Bookstore.API.Controllers;
using Bookstore.Application.Dtos;
using Bookstore.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Bookstore.Tests.Controllers
{
    public class CategoryControllerTests
    {
        private readonly Mock<ICategoryService> _categoryServiceMock;
        private readonly CategoryController _controller;

        public CategoryControllerTests()
        {
            // Khởi tạo mock cho ICategoryService
            _categoryServiceMock = new Mock<ICategoryService>();

            // Khởi tạo controller
            _controller = new CategoryController(_categoryServiceMock.Object);
        }

        [Fact]
        public async Task GetAllCategories_ReturnsOk_WithCategories()
        {
            // Arrange
            var categories = new List<CategoryDto>
            {
                new CategoryDto { CategoryId = "67eacc121af224f20dca6884", Name = "Machine Learning" },
                new CategoryDto { CategoryId = "67eacc121af224f20dca688d", Name = "Web Development" },
                new CategoryDto { CategoryId = "67eacc121af224f20dca6881", Name = "Computer Science" }
            };
            _categoryServiceMock.Setup(s => s.GetAllCategoriesAsync()).ReturnsAsync(categories);

            // Act
            var result = await _controller.GetAllCategories();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedCategories = Assert.IsType<List<CategoryDto>>(okResult.Value);
            Assert.Equal(3, returnedCategories.Count);
            Assert.Equal("Machine Learning", returnedCategories[0].Name);
            Assert.Equal("Web Development", returnedCategories[1].Name);
            Assert.Equal("Computer Science", returnedCategories[2].Name);
        }

        [Fact]
        public async Task GetAllCategories_ReturnsStatusCode500_WhenExceptionThrown()
        {
            // Arrange
            _categoryServiceMock.Setup(s => s.GetAllCategoriesAsync())
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.GetAllCategories();

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            var problemDetails = Assert.IsType<ProblemDetails>(statusCodeResult.Value);
            Assert.Equal("Đã xảy ra lỗi khi lấy danh sách danh mục", problemDetails.Title);
            Assert.Equal("Database error", problemDetails.Detail);
        }

        [Fact]
        public async Task GetCategoryById_ReturnsOk_WithCategory()
        {
            // Arrange
            var categoryId = "67eacc121af224f20dca6884";
            var category = new CategoryDto { CategoryId = categoryId, Name = "Machine Learning" };
            _categoryServiceMock.Setup(s => s.GetCategoryByIdAsync(categoryId)).ReturnsAsync(category);

            // Act
            var result = await _controller.GetCategoryById(categoryId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedCategory = Assert.IsType<CategoryDto>(okResult.Value);
            Assert.Equal(categoryId, returnedCategory.CategoryId);
            Assert.Equal("Machine Learning", returnedCategory.Name);
        }


        [Fact]
        public async Task CreateCategory_ReturnsCreated_WithValidDto()
        {
            // Arrange
            var dto = new CategoryCreateDto { Name = "New Category" };
            var createdCategory = new CategoryDto { CategoryId = "67eacc121af224f20dca6890", Name = "New Category" };
            _categoryServiceMock.Setup(s => s.CreateCategoryAsync(dto)).ReturnsAsync(createdCategory);

            // Act
            var result = await _controller.CreateCategory(dto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal("GetCategoryById", createdResult.ActionName);
            Assert.Equal(createdCategory.CategoryId, createdResult.RouteValues["categoryid"]);
            var returnedCategory = Assert.IsType<CategoryDto>(createdResult.Value);
            Assert.Equal(createdCategory.CategoryId, returnedCategory.CategoryId);
            Assert.Equal("New Category", returnedCategory.Name);
        }


        [Fact]
        public async Task CreateCategory_ReturnsStatusCode500_WhenExceptionThrown()
        {
            // Arrange
            var dto = new CategoryCreateDto { Name = "New Category" };
            _categoryServiceMock.Setup(s => s.CreateCategoryAsync(dto))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.CreateCategory(dto);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            var problemDetails = Assert.IsType<ProblemDetails>(statusCodeResult.Value);
            Assert.Equal("Đã xảy ra lỗi khi tạo danh mục", problemDetails.Title);
            Assert.Equal("Database error", problemDetails.Detail);
        }

        [Fact]
        public async Task UpdateCategory_ReturnsOk_WithValidInput()
        {
            // Arrange
            var categoryId = "67eacc121af224f20dca6884";
            var dto = new CategoryUpdateDto { Name = "Updated Machine Learning" };
            var updatedCategory = new CategoryDto { CategoryId = categoryId, Name = "Updated Machine Learning" };
            _categoryServiceMock.Setup(s => s.UpdateCategoryAsync(categoryId, dto)).ReturnsAsync(updatedCategory);

            // Act
            var result = await _controller.UpdateCategory(categoryId, dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedCategory = Assert.IsType<CategoryDto>(okResult.Value);
            Assert.Equal(categoryId, returnedCategory.CategoryId);
            Assert.Equal("Updated Machine Learning", returnedCategory.Name);
        }



        [Fact]
        public async Task UpdateCategory_ReturnsStatusCode500_WhenExceptionThrown()
        {
            // Arrange
            var categoryId = "67eacc121af224f20dca6884";
            var dto = new CategoryUpdateDto { Name = "Updated Machine Learning" };
            _categoryServiceMock.Setup(s => s.UpdateCategoryAsync(categoryId, dto))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.UpdateCategory(categoryId, dto);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            var problemDetails = Assert.IsType<ProblemDetails>(statusCodeResult.Value);
            Assert.Equal("Đã xảy ra lỗi khi cập nhật danh mục", problemDetails.Title);
            Assert.Equal("Database error", problemDetails.Detail);
        }

        [Fact]
        public async Task DeleteCategory_ReturnsNoContent_WhenSuccessful()
        {
            // Arrange
            var categoryId = "67eacc121af224f20dca6884";
            _categoryServiceMock.Setup(s => s.DeleteCategoryAsync(categoryId)).ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteCategory(categoryId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteCategory_ReturnsNotFound_WhenCategoryDoesNotExist()
        {
            // Arrange
            var categoryId = "non-existent-id";
            _categoryServiceMock.Setup(s => s.DeleteCategoryAsync(categoryId)).ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteCategory(categoryId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var problemDetails = Assert.IsType<ProblemDetails>(notFoundResult.Value);
            Assert.Equal("Không tìm thấy danh mục", problemDetails.Title);
            Assert.Equal($"Danh mục với ID {categoryId} không tồn tại", problemDetails.Detail);
        }


        [Fact]
        public async Task DeleteCategory_ReturnsStatusCode500_WhenExceptionThrown()
        {
            // Arrange
            var categoryId = "67eacc121af224f20dca6884";
            _categoryServiceMock.Setup(s => s.DeleteCategoryAsync(categoryId))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.DeleteCategory(categoryId);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            var problemDetails = Assert.IsType<ProblemDetails>(statusCodeResult.Value);
            Assert.Equal("Đã xảy ra lỗi khi xóa danh mục", problemDetails.Title);
            Assert.Equal("Database error", problemDetails.Detail);
        }
    }
}