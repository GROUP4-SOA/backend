using Bookstore.Application.Dtos;
using Bookstore.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Bookstore.API.Controllers
{
    [Route("api/categories")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(ICategoryService categoryService, ILogger<CategoryController> logger)
        {
            _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<CategoryDto>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetAllCategories()
        {
            try
            {
                var categories = await _categoryService.GetAllCategoriesAsync();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách danh mục");
                return StatusCode(500, new ProblemDetails
                {
                    Status = 500,
                    Title = "Đã xảy ra lỗi khi lấy danh sách danh mục",
                    Detail = ex.Message
                });
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CategoryDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CategoryDto>> GetCategoryById(string id)
        {
            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(id);
                return Ok(category);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new ProblemDetails
                {
                    Status = 404,
                    Title = "Không tìm thấy danh mục",
                    Detail = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh mục với ID: {Id}", id);
                return StatusCode(500, new ProblemDetails
                {
                    Status = 500,
                    Title = "Đã xảy ra lỗi khi lấy thông tin danh mục",
                    Detail = ex.Message
                });
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(CategoryDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CategoryDto>> CreateCategory([FromBody] CategoryCreateDto categoryCreateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var createdCategory = await _categoryService.CreateCategoryAsync(categoryCreateDto);
                return CreatedAtAction(nameof(GetCategoryById), 
                    new { id = createdCategory.CategoryId }, 
                    createdCategory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo danh mục mới");
                return StatusCode(500, new ProblemDetails
                {
                    Status = 500,
                    Title = "Đã xảy ra lỗi khi tạo danh mục",
                    Detail = ex.Message
                });
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CategoryDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CategoryDto>> UpdateCategory(string id, [FromBody] CategoryUpdateDto categoryUpdateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var updatedCategory = await _categoryService.UpdateCategoryAsync(id, categoryUpdateDto);
                return Ok(updatedCategory);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new ProblemDetails
                {
                    Status = 404,
                    Title = "Không tìm thấy danh mục",
                    Detail = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật danh mục với ID: {Id}", id);
                return StatusCode(500, new ProblemDetails
                {
                    Status = 500,
                    Title = "Đã xảy ra lỗi khi cập nhật danh mục",
                    Detail = ex.Message
                });
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteCategory(string id)
        {
            try
            {
                var result = await _categoryService.DeleteCategoryAsync(id);
                if (!result)
                    return NotFound(new ProblemDetails
                    {
                        Status = 404,
                        Title = "Không tìm thấy danh mục",
                        Detail = $"Danh mục với ID {id} không tồn tại"
                    });

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return NotFound(new ProblemDetails
                {
                    Status = 404,
                    Title = "Không tìm thấy danh mục",
                    Detail = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa danh mục với ID: {Id}", id);
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