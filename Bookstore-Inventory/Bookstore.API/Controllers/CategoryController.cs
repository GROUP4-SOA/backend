using Bookstore.Application.Dtos;
using Bookstore.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Bookstore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCategories()
        {
            try
            {
                var categories = await _categoryService.GetAllCategoriesAsync();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ProblemDetails
                {
                    Status = 500,
                    Title = "Đã xảy ra lỗi khi lấy danh sách danh mục",
                    Detail = ex.Message
                });
            }
        }

        [HttpGet("{categoryid}")]
        public async Task<IActionResult> GetCategoryById(string categoryid)
        {
            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(categoryid);
                return Ok(category);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy sách", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryCreateDto categoryCreateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var createdCategory = await _categoryService.CreateCategoryAsync(categoryCreateDto);
                return CreatedAtAction(nameof(GetCategoryById), 
                    new { categoryid = createdCategory.CategoryId }, 
                    createdCategory);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ProblemDetails
                {
                    Status = 500,
                    Title = "Đã xảy ra lỗi khi tạo danh mục",
                    Detail = ex.Message
                });
            }
        }

        [HttpPut("{categoryid}")]
        public async Task<IActionResult> UpdateCategory(string categoryid, [FromBody] CategoryUpdateDto categoryUpdateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var updatedCategory = await _categoryService.UpdateCategoryAsync(categoryid, categoryUpdateDto);
                return Ok(updatedCategory);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ProblemDetails
                {
                    Status = 500,
                    Title = "Đã xảy ra lỗi khi cập nhật danh mục",
                    Detail = ex.Message
                });
            }
        }

        [HttpDelete("{categoryid}")]
        public async Task<IActionResult> DeleteCategory(string categoryid)
        {
            try
            {
                var result = await _categoryService.DeleteCategoryAsync(categoryid);
                if (!result)
                    return NotFound(new ProblemDetails
                    {
                        Status = 404,
                        Title = "Không tìm thấy danh mục",
                        Detail = $"Danh mục với ID {categoryid} không tồn tại"
                    });

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ProblemDetails
                {
                    Status = 500,
                    Title = "Đã xảy ra lỗi khi xóa danh mục",
                    Detail = ex.Message
                });
            }
        }
    }
}