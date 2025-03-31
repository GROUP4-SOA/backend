using Bookstore.Application.Dtos;
using Bookstore.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Bookstore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookController : ControllerBase
    {
        private readonly IBookService _bookService;

        public BookController(IBookService bookService)
        {
            _bookService = bookService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllBooks()
        {
            try
            {
                var books = await _bookService.GetAllBooksAsync();
                return Ok(books);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy danh sách sách", error = ex.Message });
            }
        }

        [HttpGet("{bookId}")]
        public async Task<IActionResult> GetBookById(string bookId)
        {
            try
            {
                var book = await _bookService.GetBookByIdAsync(bookId);
                return Ok(book);
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
        public async Task<IActionResult> CreateBook([FromBody] BookCreateDto bookCreateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var createdBook = await _bookService.CreateBookAsync(bookCreateDto);
                return CreatedAtAction(nameof(GetBookById), new { bookId = createdBook.BookId }, createdBook);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi tạo sách", error = ex.Message });
            }
        }

        [HttpPut("{bookId}")]
        public async Task<IActionResult> UpdateBook(string bookId, [FromBody] BookUpdateDto bookUpdateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var updatedBook = await _bookService.UpdateBookAsync(bookId, bookUpdateDto);
                return Ok(updatedBook);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi cập nhật sách", error = ex.Message });
            }
        }

        [HttpDelete("{bookId}")]
        public async Task<IActionResult> DeleteBook(string bookId)
        {
            try
            {
                var result = await _bookService.DeleteBookAsync(bookId);
                if (!result)
                    return NotFound(new { message = $"Sách với ID {bookId} không tồn tại" });

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi xóa sách", error = ex.Message });
            }
        }
    }
}