using Bookstore.Application.Dtos;
using Bookstore.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Bookstore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImportController : ControllerBase
    {
        private readonly IImportService _importService;

        public ImportController(IImportService importService)
        {
            _importService = importService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateImport([FromBody] WarehouseImportDto importDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var createdImport = await _importService.CreateImportAsync(importDto);
                return CreatedAtAction(nameof(GetAllImports), new { importId = createdImport.ImportId }, createdImport);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi tạo phiếu nhập kho", error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllImports()
        {
            try
            {
                var imports = await _importService.GetAllImportsAsync();
                return Ok(imports);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy danh sách phiếu nhập kho", error = ex.Message });
            }
        }
    }
}