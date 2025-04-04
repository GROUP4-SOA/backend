using Bookstore.Application.Dtos;
using Bookstore.Application.Interfaces.Services; // Assuming IExportService is defined here
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using Xunit;

namespace Bookstore.Tests.Controllers
{
    public class ExportControllerTests
    {
        private readonly Mock<IExportService> _exportServiceMock;

        public ExportControllerTests()
        {
            _exportServiceMock = new Mock<IExportService>();
        }

        [Fact]
        public async Task GetAllExports_ReturnsOk_WithExports()
        {
            // Arrange
            var exports = new List<WarehouseExportDto> { new() { ExportId = "67eae55c1af224f20dca6929", UserId = "67eacf021af224f20dca689e", ExportDate = DateTime.UtcNow } };
            _exportServiceMock.Setup(s => s.GetAllExportsAsync()).ReturnsAsync(exports);
            Func<IExportService, Task<IResult>> getAllExports = async (service) => Results.Ok(await service.GetAllExportsAsync());

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
            var dto = new WarehouseExportDto { ExportId = "exp1", UserId = "67eacf021af224f20dca689e", ExportDate = DateTime.UtcNow, WarehouseExportBooks = new List<WarehouseExportBookDto> { new() { BookId = "1", ExportQuantity = 5 } } };
            _exportServiceMock.Setup(s => s.CreateExportAsync(dto)).ReturnsAsync(dto);
            Func<WarehouseExportDto, IExportService, Task<IResult>> createExport = async (dto, service) =>
            {
                try
                {
                    var created = await service.CreateExportAsync(dto);
                    return Results.Created($"/api/warehouse-exports/{created.ExportId}", created);
                }
                catch (ArgumentException ex)
                {
                    return Results.BadRequest(new { message = ex.Message });
                }
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