using Bookstore.Application.Dtos;
using Bookstore.Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using MongoDB.Driver;
using Moq;
using Xunit;

namespace Bookstore.Tests.Controllers
{
    public class ExportControllerTests
    {
        private readonly Mock<ExportService> _exportServiceMock;

        public ExportControllerTests()
        {
            _exportServiceMock = new Mock<ExportService>(Mock.Of<IMongoDatabase>());
        }

        [Fact]
        public async Task GetAllExports_ReturnsOk_WithExports()
        {
            // Arrange
            var exports = new List<WarehouseExportDto> { new() { ExportId = "exp1", UserId = "user1", ExportDate = DateTime.UtcNow } };
            _exportServiceMock.Setup(s => s.GetAllExportsAsync()).ReturnsAsync(exports);
            Func<ExportService, Task<IResult>> getAllExports = async (service) => Results.Ok(await service.GetAllExportsAsync());

            // Act
            var result = await getAllExports(_exportServiceMock.Object);

            // Assert
            var okResult = Assert.IsType<Ok<List<WarehouseExportDto>>>(result);
            Assert.Equal(exports, okResult.Value);
        }

        [Fact]
        public async Task CreateExport_ReturnsCreated_WithValidDto()
        {
            // Arrange
            var dto = new WarehouseExportDto { ExportId = "exp1", UserId = "user1", ExportDate = DateTime.UtcNow, WarehouseExportBooks = new List<WarehouseExportBookDto> { new() { BookId = "1", ExportQuantity = 5 } } };
            _exportServiceMock.Setup(s => s.CreateExportAsync(dto)).ReturnsAsync(dto);
            Func<WarehouseExportDto, ExportService, Task<IResult>> createExport = async (dto, service) =>
            {
                var created = await service.CreateExportAsync(dto);
                return Results.Created($"/api/warehouse-exports/{created.ExportId}", created);
            };

            // Act
            var result = await createExport(dto, _exportServiceMock.Object);

            // Assert
            var createdResult = Assert.IsType<Created<WarehouseExportDto>>(result);
            Assert.Equal($"/api/warehouse-exports/{dto.ExportId}", createdResult.Location);
            Assert.Equal(dto, createdResult.Value);
        }
    }
}