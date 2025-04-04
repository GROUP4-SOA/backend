using Bookstore.Infrastructure.Data;
using Bookstore.Infrastructure.Repositories;
using Bookstore.Domain.Entities;
using MongoDB.Driver;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Bookstore.Infrastructure.Tests.Repositories
{
    public class CategoryRepositoryTests
    {
        private readonly Mock<IMongoCollection<Category>> _mockCollection;
        private readonly Mock<MongoDbContext> _mockDbContext;
        private readonly CategoryRepository _repository;

        public CategoryRepositoryTests()
        {
            // Khởi tạo mock cho IMongoCollection
            _mockCollection = new Mock<IMongoCollection<Category>>();

            // Khởi tạo mock cho MongoDbContext
            _mockDbContext = new Mock<MongoDbContext>();
            _mockDbContext
                .Setup(db => db.GetCollection<Category>(It.IsAny<string>()))
                .Returns(_mockCollection.Object);

            // Khởi tạo repository
            _repository = new CategoryRepository(_mockDbContext.Object);
        }

        [Fact]
        public async Task GetAllCategoriesAsync_ShouldReturnAllCategories()
        {
            // Arrange
            var categories = new List<Category>
            {
                new Category
                {
                    CategoryId = "67eacc121af224f20dca6884",
                    Name = "Machine Learning"
                },
                new Category
                {
                    CategoryId = "67eacc121af224f20dca688d",
                    Name = "Web Development"
                },
                new Category
                {
                    CategoryId = "67eacc121af224f20dca6881",
                    Name = "Computer Science"
                }
            };

            var mockCursor = new Mock<IAsyncCursor<Category>>();
            mockCursor.Setup(_ => _.Current).Returns(categories);
            mockCursor
                .SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            _mockCollection
                .Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<Category>>(),
                    It.IsAny<FindOptions<Category, Category>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            // Act
            var result = await _repository.GetAllCategoriesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Equal("Machine Learning", result[0].Name);
            Assert.Equal("Web Development", result[1].Name);
            Assert.Equal("Computer Science", result[2].Name);
        }

        [Fact]
        public async Task GetCategoryByIdAsync_ShouldReturnCategory_WhenIdExists()
        {
            // Arrange
            var categoryId = "67eacc121af224f20dca6884";
            var category = new Category
            {
                CategoryId = categoryId,
                Name = "Machine Learning"
            };

            var mockCursor = new Mock<IAsyncCursor<Category>>();
            mockCursor.Setup(_ => _.Current).Returns(new List<Category> { category });
            mockCursor
                .SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            _mockCollection
                .Setup(c => c.FindAsync(
                    It.Is<FilterDefinition<Category>>(f => f == Builders<Category>.Filter.Eq(x => x.CategoryId, categoryId)),
                    It.IsAny<FindOptions<Category, Category>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            // Act
            var result = await _repository.GetCategoryByIdAsync(categoryId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(categoryId, result.CategoryId);
            Assert.Equal("Machine Learning", result.Name);
        }

        [Fact]
        public async Task GetCategoryByIdAsync_ShouldReturnNull_WhenIdDoesNotExist()
        {
            // Arrange
            var categoryId = "non-existent-id";

            var mockCursor = new Mock<IAsyncCursor<Category>>();
            mockCursor.Setup(_ => _.Current).Returns(new List<Category>());
            mockCursor
                .SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockCollection
                .Setup(c => c.FindAsync(
                    It.Is<FilterDefinition<Category>>(f => f == Builders<Category>.Filter.Eq(x => x.CategoryId, categoryId)),
                    It.IsAny<FindOptions<Category, Category>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            // Act
            var result = await _repository.GetCategoryByIdAsync(categoryId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AddCategoryAsync_ShouldInsertCategory()
        {
            // Arrange
            var category = new Category
            {
                CategoryId = "67eacc121af224f20dca6884",
                Name = "Machine Learning"
            };

            _mockCollection
                .Setup(c => c.InsertOneAsync(
                    It.IsAny<Category>(),
                    It.IsAny<InsertOneOptions>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _repository.AddCategoryAsync(category);

            // Assert
            _mockCollection.Verify(c => c.InsertOneAsync(category, null, default), Times.Once());
        }

        [Fact]
        public async Task UpdateCategoryAsync_ShouldReplaceCategory()
        {
            // Arrange
            var category = new Category
            {
                CategoryId = "67eacc121af224f20dca6884",
                Name = "Machine Learning Updated"
            };

            _mockCollection
                .Setup(c => c.ReplaceOneAsync(
                    It.IsAny<FilterDefinition<Category>>(),
                    It.IsAny<Category>(),
                    It.IsAny<ReplaceOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ReplaceOneResult.Acknowledged(1, 1, null));

            // Act
            await _repository.UpdateCategoryAsync(category);

            // Assert
            _mockCollection.Verify(c => c.ReplaceOneAsync(
                It.IsAny<FilterDefinition<Category>>(),
                category,
                It.IsAny<ReplaceOptions>(),
                default), Times.Once());
        }

        [Fact]
        public async Task DeleteCategoryAsync_ShouldRemoveCategory()
        {
            // Arrange
            var categoryId = "67eacc121af224f20dca6884";

            _mockCollection
                .Setup(c => c.DeleteOneAsync(
                    It.IsAny<FilterDefinition<Category>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DeleteResult.Acknowledged(1));

            // Act
            await _repository.DeleteCategoryAsync(categoryId);

            // Assert
            _mockCollection.Verify(c => c.DeleteOneAsync(
                It.IsAny<FilterDefinition<Category>>(),
                default), Times.Once());
        }
    }
}