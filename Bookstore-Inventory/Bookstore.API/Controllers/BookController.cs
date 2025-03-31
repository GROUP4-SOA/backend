// Bookstore.API/Controllers/BookController.cs
using Bookstore.Application.DTOs;
using Bookstore.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Bookstore.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
            var books = await _bookService.GetAllBooksAsync();
            return Ok(books);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBookById(string id)
        {
            var book = await _bookService.GetBookByIdAsync(id);
            if (book == null) return NotFound();
            return Ok(book);
        }

        [HttpPost]
        public async Task<IActionResult> CreateBook([FromBody] BookCreateDto bookDto)
        {
            var book = await _bookService.CreateBookAsync(bookDto);
            return CreatedAtAction(nameof(GetBookById), new { id = book.Id }, book);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBook(string id, [FromBody] BookUpdateDto bookDto)
        {
            await _bookService.UpdateBookAsync(id, bookDto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(string id)
        {
            await _bookService.DeleteBookAsync(id);
            return NoContent();
        }
    }
}