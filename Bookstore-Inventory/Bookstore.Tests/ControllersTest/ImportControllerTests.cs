using Bookstore.Application.Dtos;
using Bookstore.Application.Interfaces.Services;
using Bookstore.Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using Xunit;

namespace Bookstore.Tests.Controllers
{
    public class ImportControllerTests
    {
        private readonly Mock<IImportService> _importServiceMock;

        public ImportControllerTests()
        {
            _importServiceMock = new Mock<IImportService>();
        }

        [Fact]
        public async Task GetAllImports_ReturnsOk_WithImports()
        {
            // Arrange
            var imports = new List<WarehouseImportDto> { new() { ImportId = "67eae55c1af224f20dca6929", UserId = "67eacf021af224f20dca689e", ImportDate = DateTime.UtcNow } };
            _importServiceMock.Setup(s => s.GetAllImportsAsync()).ReturnsAsync(imports);
            Func<IImportService, Task<IResult>> getAllImports = async (service) => Results.Ok(await service.GetAllImportsAsync());

            // Act
            var result = await getAllImports(_importServiceMock.Object);

            // Assert
            var okResult = Assert.IsType<Ok<List<WarehouseImportDto>>>(result);
            Assert.Equal(imports, okResult.Value);
        }

        [Fact]
        public async Task CreateImport_ReturnsCreated_WithValidDto()
        {
            // Arrange
            var dto = new WarehouseImportDto { ImportId = "imp1", UserId = "user1", ImportDate = DateTime.UtcNow, WarehouseImportBooks = new List<WarehouseImportBookDto> { new() { BookId = "1", ImportQuantity = 10 } } };
            _importServiceMock.Setup(s => s.CreateImportAsync(dto)).ReturnsAsync(dto);
            Func<WarehouseImportDto, IImportService, Task<IResult>> createImport = async (dto, service) =>
            {
                try
                {
                    var created = await service.CreateImportAsync(dto);
                    return Results.Created($"/api/imports/{created.ImportId}", created);
                }
                catch (ArgumentException ex)
                {
                    return Results.BadRequest(new { message = ex.Message });
                }
            };

            // Act
            var result = await createImport(dto, _importServiceMock.Object);

            // Assert
            var createdResult = Assert.IsType<Created<WarehouseImportDto>>(result);
            Assert.Equal($"/api/imports/{dto.ImportId}", createdResult.Location);
            Assert.Equal(dto, createdResult.Value);
        }
    }
}