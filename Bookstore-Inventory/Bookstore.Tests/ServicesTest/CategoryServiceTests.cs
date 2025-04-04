using Bookstore.Application.Dtos;
using Bookstore.Application.Services;
using Bookstore.Domain.Entities;
using MongoDB.Driver;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Bookstore.Tests
{
    public class CategoryServiceTests
    {
        private readonly Mock<IMongoCollection<Category>> _categoriesCollectionMock;
        private readonly CategoryService _categoryService;

        public CategoryServiceTests()
        {
            var databaseMock = new Mock<IMongoDatabase>();
            _categoriesCollectionMock = new Mock<IMongoCollection<Category>>();

            databaseMock.Setup(db => db.GetCollection<Category>("Category", null))
                .Returns(_categoriesCollectionMock.Object);

            _categoryService = new CategoryService(databaseMock.Object);
        }

        // Test GetAllCategoriesAsync
        [Fact]
        public async Task GetAllCategoriesAsync_ReturnsAllCategories()
        {
            var categories = new List<Category>
            {
                new Category { CategoryId = "cat1", Name = "Fiction" },
                new Category { CategoryId = "cat2", Name = "Non-Fiction" }
            };

            var findAllCategoriesMock = new Mock<IAsyncCursor<Category>>();
            findAllCategoriesMock.Setup(x => x.ToListAsync(It.IsAny<CancellationToken>())).ReturnsAsync(categories);

            _categoriesCollectionMock.Setup(x => x.FindAsync(It.IsAny<FilterDefinition<Category>>(), null, CancellationToken.None))
                .ReturnsAsync(findAllCategoriesMock.Object);

            var result = await _categoryService.GetAllCategoriesAsync();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains(result, c => c.CategoryId == "cat1" && c.Name == "Fiction");
            Assert.Contains(result, c => c.CategoryId == "cat2" && c.Name == "Non-Fiction");
        }

        // Test GetCategoryByIdAsync
        [Fact]
        public async Task GetCategoryByIdAsync_ValidId_ReturnsCategory()
        {
            var categoryId = "cat1";
            var category = new Category { CategoryId = categoryId, Name = "Fiction" };

            var findCategoryMock = new Mock<IAsyncCursor<Category>>();
            findCategoryMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<CancellationToken>())).ReturnsAsync(category);

            _categoriesCollectionMock.Setup(x => x.FindAsync(It.IsAny<FilterDefinition<Category>>(), null, CancellationToken.None))
                .ReturnsAsync(findCategoryMock.Object);

            var result = await _categoryService.GetCategoryByIdAsync(categoryId);

            Assert.NotNull(result);
            Assert.Equal(category.CategoryId, result.CategoryId);
            Assert.Equal(category.Name, result.Name);
        }

        [Fact]
        public async Task GetCategoryByIdAsync_NullOrEmptyId_ThrowsArgumentException()
        {
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _categoryService.GetCategoryByIdAsync(""));
            Assert.Equal("CategoryId không được để trống", exception.Message);
        }

        [Fact]
        public async Task GetCategoryByIdAsync_CategoryNotFound_ThrowsKeyNotFoundException()
        {
            var categoryId = "cat1";
            var findCategoryMock = new Mock<IAsyncCursor<Category>>();
            findCategoryMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<CancellationToken>())).ReturnsAsync((Category)null);

            _categoriesCollectionMock.Setup(x => x.FindAsync(It.IsAny<FilterDefinition<Category>>(), null, CancellationToken.None))
                .ReturnsAsync(findCategoryMock.Object);

            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _categoryService.GetCategoryByIdAsync(categoryId));
            Assert.Equal($"Không tìm thấy danh mục với CategoryId {categoryId}", exception.Message);
        }

        // Test CreateCategoryAsync
        [Fact]
        public async Task CreateCategoryAsync_ValidCategory_CreatesCategory()
        {
            var categoryCreateDto = new CategoryCreateDto { CategoryId = "cat1", Name = "Fiction" };

            var findCategoryMock = new Mock<IAsyncCursor<Category>>();
            findCategoryMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<CancellationToken>())).ReturnsAsync((Category)null);

            _categoriesCollectionMock.Setup(x => x.FindAsync(It.IsAny<FilterDefinition<Category>>(), null, CancellationToken.None))
                .ReturnsAsync(findCategoryMock.Object);
            _categoriesCollectionMock.Setup(x => x.InsertOneAsync(It.IsAny<Category>(), null, CancellationToken.None))
                .Returns(Task.CompletedTask);

            var result = await _categoryService.CreateCategoryAsync(categoryCreateDto);

            Assert.NotNull(result);
            Assert.Equal(categoryCreateDto.CategoryId, result.CategoryId);
            Assert.Equal(categoryCreateDto.Name, result.Name);
            _categoriesCollectionMock.Verify(x => x.InsertOneAsync(It.IsAny<Category>(), null, CancellationToken.None), Times.Once());
        }

        [Fact]
        public async Task CreateCategoryAsync_DuplicateCategoryId_ThrowsArgumentException()
        {
            var categoryCreateDto = new CategoryCreateDto { CategoryId = "cat1", Name = "Fiction" };
            var existingCategory = new Category { CategoryId = "cat1" };

            var findCategoryMock = new Mock<IAsyncCursor<Category>>();
            findCategoryMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<CancellationToken>())).ReturnsAsync(existingCategory);

            _categoriesCollectionMock.Setup(x => x.FindAsync(It.IsAny<FilterDefinition<Category>>(), null, CancellationToken.None))
                .ReturnsAsync(findCategoryMock.Object);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _categoryService.CreateCategoryAsync(categoryCreateDto));
            Assert.Equal($"Category với CategoryId {categoryCreateDto.CategoryId} đã tồn tại", exception.Message);
        }

        // Test UpdateCategoryAsync
        [Fact]
        public async Task UpdateCategoryAsync_ValidCategory_UpdatesCategory()
        {
            var categoryId = "cat1";
            var categoryUpdateDto = new CategoryUpdateDto { Name = "Updated Fiction" };
            var existingCategory = new Category { CategoryId = categoryId, Name = "Fiction" };

            var findCategoryMock = new Mock<IAsyncCursor<Category>>();
            findCategoryMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<CancellationToken>())).ReturnsAsync(existingCategory);

            _categoriesCollectionMock.Setup(x => x.FindAsync(It.IsAny<FilterDefinition<Category>>(), null, CancellationToken.None))
                .ReturnsAsync(findCategoryMock.Object);
            _categoriesCollectionMock.Setup(x => x.ReplaceOneAsync(It.IsAny<FilterDefinition<Category>>(), It.IsAny<Category>(), It.IsAny<ReplaceOptions>(), CancellationToken.None))
                .ReturnsAsync(new ReplaceOneResult.Acknowledged(1, 1, null));

            var result = await _categoryService.UpdateCategoryAsync(categoryId, categoryUpdateDto);

            Assert.NotNull(result);
            Assert.Equal(categoryId, result.CategoryId);
            Assert.Equal(categoryUpdateDto.Name, result.Name);
            _categoriesCollectionMock.Verify(x => x.ReplaceOneAsync(It.IsAny<FilterDefinition<Category>>(), It.IsAny<Category>(), It.IsAny<ReplaceOptions>(), CancellationToken.None), Times.Once());
        }

        [Fact]
        public async Task UpdateCategoryAsync_NullOrEmptyId_ThrowsArgumentException()
        {
            var categoryUpdateDto = new CategoryUpdateDto { Name = "Updated Fiction" };

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _categoryService.UpdateCategoryAsync("", categoryUpdateDto));
            Assert.Equal("CategoryId không được để trống", exception.Message);
        }

        [Fact]
        public async Task UpdateCategoryAsync_CategoryNotFound_ThrowsKeyNotFoundException()
        {
            var categoryId = "cat1";
            var categoryUpdateDto = new CategoryUpdateDto { Name = "Updated Fiction" };

            var findCategoryMock = new Mock<IAsyncCursor<Category>>();
            findCategoryMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<CancellationToken>())).ReturnsAsync((Category)null);

            _categoriesCollectionMock.Setup(x => x.FindAsync(It.IsAny<FilterDefinition<Category>>(), null, CancellationToken.None))
                .ReturnsAsync(findCategoryMock.Object);

            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _categoryService.UpdateCategoryAsync(categoryId, categoryUpdateDto));
            Assert.Equal($"Không tìm thấy danh mục với CategoryId {categoryId}", exception.Message);
        }

        // Test DeleteCategoryAsync
        [Fact]
        public async Task DeleteCategoryAsync_ValidId_DeletesCategory()
        {
            var categoryId = "cat1";

            _categoriesCollectionMock.Setup(x => x.DeleteOneAsync(It.IsAny<FilterDefinition<Category>>(), CancellationToken.None))
                .ReturnsAsync(new DeleteResult.Acknowledged(1));

            var result = await _categoryService.DeleteCategoryAsync(categoryId);

            Assert.True(result);
            _categoriesCollectionMock.Verify(x => x.DeleteOneAsync(It.IsAny<FilterDefinition<Category>>(), CancellationToken.None), Times.Once());
        }

        [Fact]
        public async Task DeleteCategoryAsync_NullOrEmptyId_ThrowsArgumentException()
        {
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _categoryService.DeleteCategoryAsync(""));
            Assert.Equal("CategoryId không được để trống", exception.Message);
        }

        [Fact]
        public async Task DeleteCategoryAsync_CategoryNotFound_ReturnsFalse()
        {
            var categoryId = "cat1";

            _categoriesCollectionMock.Setup(x => x.DeleteOneAsync(It.IsAny<FilterDefinition<Category>>(), CancellationToken.None))
                .ReturnsAsync(new DeleteResult.Acknowledged(0));

            var result = await _categoryService.DeleteCategoryAsync(categoryId);

            Assert.False(result);
        }
    }
}