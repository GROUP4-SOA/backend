using Bookstore.Application.Dtos;
using Bookstore.Application.Interfaces.Services;
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
                return StatusCode(500, new { message = "An error occurred while retrieving categories", error = ex.Message });
            }
        }

        [HttpGet("{categoryId}")]
        public async Task<IActionResult> GetCategoryById(string categoryId)
        {
            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(categoryId);
                if (category == null)
                {
                    return NotFound(new { message = "Category not found" });
                }
                return Ok(category);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the category", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryDto categoryDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var createdCategory = await _categoryService.CreateCategoryAsync(categoryDto);
                return CreatedAtAction(nameof(GetCategoryById), new { categoryId = createdCategory.CategoryId }, createdCategory);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the category", error = ex.Message });
            }
        }

        [HttpPut("{categoryId}")]
        public async Task<IActionResult> UpdateCategory(string categoryId, [FromBody] CategoryDto categoryDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var updatedCategory = await _categoryService.UpdateCategoryAsync(categoryId, categoryDto);
                if (updatedCategory == null)
                {
                    return NotFound(new { message = "Category not found" });
                }
                return Ok(updatedCategory);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the category", error = ex.Message });
            }
        }

        [HttpDelete("{categoryId}")]
        public async Task<IActionResult> DeleteCategory(string categoryId)
        {
            try
            {
                var result = await _categoryService.DeleteCategoryAsync(categoryId);
                if (!result)
                {
                    return NotFound(new { message = "Category not found" });
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the category", error = ex.Message });
            }
        }
    }
}