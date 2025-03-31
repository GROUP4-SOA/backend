using Bookstore.Application.Dtos;
using Bookstore.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Bookstore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExportController : ControllerBase
    {
        private readonly IExportService _exportService;

        public ExportController(IExportService exportService)
        {
            _exportService = exportService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateExport([FromBody] WarehouseExportDto exportDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var createdExport = await _exportService.CreateExportAsync(exportDto);
                return CreatedAtAction(nameof(GetAllExports), new { exportId = createdExport.ExportId }, createdExport);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi tạo phiếu xuất kho", error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllExports()
        {
            try
            {
                var exports = await _exportService.GetAllExportsAsync();
                return Ok(exports);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy danh sách phiếu xuất kho", error = ex.Message });
            }
        }
    }
}