using Bookstore.Application.Dtos;
using Bookstore.Application.Services;
using Bookstore.Domain.Entities;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
            // Arrange
            var categories = new List<Category>
            {
                new Category { Id = "67eacc121af224f20dca6884", Name = "Machine Learning", Description = "Books about machine learning" },
                new Category { Id = "67eacc121af224f20dca688d", Name = "Web Development", Description = "Books about web development" }
            };

            var mockCursor = new Mock<IAsyncCursor<Category>>();
            mockCursor.Setup(_ => _.Current).Returns(categories);
            mockCursor
                .SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            _categoriesCollectionMock
                .Setup(x => x.FindAsync(
                    It.IsAny<FilterDefinition<Category>>(),
                    It.IsAny<FindOptions<Category, Category>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            // Act
            var result = await _categoryService.GetAllCategoriesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains(result, c => c.CategoryId == "67eacc121af224f20dca6884" && c.Name == "Machine Learning" && c.Description == "Books about machine learning");
            Assert.Contains(result, c => c.CategoryId == "67eacc121af224f20dca688d" && c.Name == "Web Development" && c.Description == "Books about web development");
        }

        [Fact]
        public async Task GetCategoryByIdAsync_ValidId_ReturnsCategory()
        {
            // Arrange
            var categoryId = "67eacc121af224f20dca6884";
            var category = new Category { Id = categoryId, Name = "Machine Learning", Description = "Books about machine learning" };

            // Mock FindAsync
            var mockCursor = new Mock<IAsyncCursor<Category>>();
            mockCursor.Setup(_ => _.Current).Returns(new List<Category> { category });
            mockCursor.SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            _categoriesCollectionMock
                .Setup(x => x.FindAsync(
                    It.IsAny<FilterDefinition<Category>>(),
                    It.IsAny<FindOptions<Category, Category>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            // Act
            var result = await _categoryService.GetCategoryByIdAsync(categoryId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(category.Id, result.CategoryId);
            Assert.Equal(category.Name, result.Name);
            Assert.Equal(category.Description, result.Description);
        }


        [Fact]
        public async Task GetCategoryByIdAsync_NullOrEmptyId_ThrowsArgumentException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _categoryService.GetCategoryByIdAsync(""));
            Assert.Equal("CategoryId không được để trống", exception.Message);
        }

        // Test CreateCategoryAsync
        [Fact]
        public async Task CreateCategoryAsync_ValidCategory_CreatesCategory()
        {
            // Arrange
            var categoryCreateDto = new CategoryCreateDto { Name = "New Category" };

            _categoriesCollectionMock
                .Setup(x => x.InsertOneAsync(
                    It.IsAny<Category>(),
                    It.IsAny<InsertOneOptions>(),
                    It.IsAny<CancellationToken>()))
                .Callback<Category, InsertOneOptions, CancellationToken>((category, options, token) =>
                {
                    category.Id = "67eacc121af224f20dca6890"; // Set the Id on the actual category object
                })
                .Returns(Task.CompletedTask);

            // Act
            var result = await _categoryService.CreateCategoryAsync(categoryCreateDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("67eacc121af224f20dca6890", result.CategoryId);
            Assert.Equal(categoryCreateDto.Name, result.Name);
            _categoriesCollectionMock.Verify(x => x.InsertOneAsync(It.IsAny<Category>(), null, CancellationToken.None), Times.Once());
        }

        // Test UpdateCategoryAsync
        [Fact]
        public async Task UpdateCategoryAsync_ValidCategory_UpdatesCategory()
        {
            // Arrange
            var categoryId = "67eacc121af224f20dca6884";
            var categoryUpdateDto = new CategoryUpdateDto { Name = "Updated Machine Learning" };
            var existingCategory = new Category { Id = categoryId, Name = "Machine Learning" };

            // Mock FindAsync để trả về existingCategory
            var mockCursor = new Mock<IAsyncCursor<Category>>();
            mockCursor.Setup(_ => _.Current).Returns(new List<Category> { existingCategory });
            mockCursor.SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            _categoriesCollectionMock
                .Setup(x => x.FindAsync(
                    It.IsAny<FilterDefinition<Category>>(),
                    It.IsAny<FindOptions<Category, Category>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            // Mock ReplaceOneAsync
            _categoriesCollectionMock
                .Setup(x => x.ReplaceOneAsync(
                    It.IsAny<FilterDefinition<Category>>(),
                    It.Is<Category>(c => c.Id == categoryId && c.Name == categoryUpdateDto.Name),
                    It.IsAny<ReplaceOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ReplaceOneResult.Acknowledged(1, 1, null));

            // Act
            var result = await _categoryService.UpdateCategoryAsync(categoryId, categoryUpdateDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(categoryId, result.CategoryId);
            Assert.Equal(categoryUpdateDto.Name, result.Name);
            _categoriesCollectionMock.Verify(x => x.ReplaceOneAsync(
                It.IsAny<FilterDefinition<Category>>(),
                It.IsAny<Category>(),
                It.IsAny<ReplaceOptions>(),
                It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task UpdateCategoryAsync_NullOrEmptyId_ThrowsArgumentException()
        {
            // Arrange
            var categoryUpdateDto = new CategoryUpdateDto { Name = "Updated Machine Learning" };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _categoryService.UpdateCategoryAsync("", categoryUpdateDto));
            Assert.Equal("CategoryId không được để trống", exception.Message);
        }


        // Test DeleteCategoryAsync
        [Fact]
        public async Task DeleteCategoryAsync_ValidId_DeletesCategory()
        {
            // Arrange
            var categoryId = "67eacc121af224f20dca6884";

            _categoriesCollectionMock
                .Setup(x => x.DeleteOneAsync(
                    It.IsAny<FilterDefinition<Category>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DeleteResult.Acknowledged(1));

            // Act
            var result = await _categoryService.DeleteCategoryAsync(categoryId);

            // Assert
            Assert.True(result);
            _categoriesCollectionMock.Verify(x => x.DeleteOneAsync(It.IsAny<FilterDefinition<Category>>(), CancellationToken.None), Times.Once());
        }

        [Fact]
        public async Task DeleteCategoryAsync_NullOrEmptyId_ThrowsArgumentException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _categoryService.DeleteCategoryAsync(""));
            Assert.Equal("CategoryId không được để trống", exception.Message);
        }

        [Fact]
        public async Task DeleteCategoryAsync_CategoryNotFound_ReturnsFalse()
        {
            // Arrange
            var categoryId = "non-existent-id";

            _categoriesCollectionMock
                .Setup(x => x.DeleteOneAsync(
                    It.IsAny<FilterDefinition<Category>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DeleteResult.Acknowledged(0));

            // Act
            var result = await _categoryService.DeleteCategoryAsync(categoryId);

            // Assert
            Assert.False(result);
        }
    }
}