using Bookstore.Application.Dtos;
using Bookstore.Application.Services;
using Bookstore.Infrastructure.Data;
using Bookstore.Application.Dtos;
using Bookstore.Infrastructure.Interfaces.Repositories;
using Bookstore.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Load MongoDB settings from configuration
var mongoDbSettings = builder.Configuration.GetSection("DatabaseSettings").Get<MongoDbSettings>();
if (mongoDbSettings == null || string.IsNullOrEmpty(mongoDbSettings.ConnectionString))
{
    throw new InvalidOperationException("MongoDB settings are not properly configured.");
}

// Register MongoDB services
var mongoClient = new MongoClient(mongoDbSettings.ConnectionString);
builder.Services.AddSingleton<IMongoClient>(mongoClient);
builder.Services.AddSingleton<IMongoDatabase>(sp => 
    mongoClient.GetDatabase(mongoDbSettings.DatabaseName));

// Register application services
builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddSingleton<IBookRepository, BookRepository>();
builder.Services.AddSingleton<BookService>();
builder.Services.AddSingleton<ICategoryRepository, CategoryRepository>();
builder.Services.AddSingleton<CategoryService>();

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Bookstore API", Version = "v1" });
});

var app = builder.Build();

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseHsts();
}

// Enable CORS
app.UseCors("CorsPolicy");

// Enable Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Bookstore API V1");
    c.RoutePrefix = "swagger";
});

// API Endpoints
// Book
app.MapGet("/api/books", async (BookService service) =>
{
    try
    {
        var books = await service.GetAllBooksAsync();
        return Results.Ok(books);
    }
    catch (Exception ex)
    {
        return Results.Problem(
            statusCode: 500,
            detail: ex.Message,
            title: "Lỗi khi lấy danh sách sách"
        );
    }
});

app.MapGet("/api/books/{bookId}", async (string bookId, BookService service) =>
{
    try
    {
        var book = await service.GetBookByIdAsync(bookId);
        return Results.Ok(book);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
    catch (Exception ex)
    {
        return Results.Problem(
            statusCode: 500,
            detail: ex.Message,
            title: "Lỗi khi lấy sách"
        );
    }
});

app.MapPost("/api/books", async (BookCreateDto bookCreateDto, BookService service) =>
{
    try
    {
        var createdBook = await service.CreateBookAsync(bookCreateDto);
        return Results.Created($"/api/books/{createdBook.BookId}", createdBook);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
    catch (Exception ex)
    {
        return Results.Problem(
            statusCode: 500,
            detail: ex.Message,
            title: "Lỗi khi tạo sách"
        );
    }
});

app.MapPut("/api/books/{bookId}", async (string bookId, BookUpdateDto bookUpdateDto, BookService service) =>
{
    try
    {
        var updatedBook = await service.UpdateBookAsync(bookId, bookUpdateDto);
        return Results.Ok(updatedBook);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
    catch (Exception ex)
    {
        return Results.Problem(
            statusCode: 500,
            detail: ex.Message,
            title: "Lỗi khi cập nhật sách"
        );
    }
});

app.MapDelete("/api/books/{bookId}", async (string bookId, BookService service) =>
{
    try
    {
        var result = await service.DeleteBookAsync(bookId);
        if (!result)
            return Results.NotFound(new { message = $"Sách với ID {bookId} không tồn tại" });

        return Results.NoContent();
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
    catch (Exception ex)
    {
        return Results.Problem(
            statusCode: 500,
            detail: ex.Message,
            title: "Lỗi khi xóa sách"
        );
    }
});
// Category API Endpoints
app.MapGet("/api/categories", async (CategoryService service) =>
{
    try
    {
        var categories = await service.GetAllCategoriesAsync();
        return Results.Ok(categories);
    }
    catch (Exception ex)
    {
        return Results.Problem(
            statusCode: 500,
            detail: ex.Message,
            title: "Lỗi khi lấy danh sách danh mục"
        );
    }
});

app.MapGet("/api/categories/{categoryId}", async (string categoryId, CategoryService service) =>
{
    try
    {
        var category = await service.GetCategoryByIdAsync(categoryId);
        return Results.Ok(category);
    }
    catch (ArgumentException ex)
    {
        return Results.NotFound(new { message = ex.Message });
    }
    catch (Exception ex)
    {
        return Results.Problem(
            statusCode: 500,
            detail: ex.Message,
            title: "Lỗi khi lấy danh mục"
        );
    }
});

app.MapPost("/api/categories", async (CategoryCreateDto categoryCreateDto, CategoryService service) =>
{
    try
    {
        var createdCategory = await service.CreateCategoryAsync(categoryCreateDto);
        return Results.Created($"/api/categories/{createdCategory.CategoryId}", createdCategory);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
    catch (Exception ex)
    {
        return Results.Problem(
            statusCode: 500,
            detail: ex.Message,
            title: "Lỗi khi tạo danh mục"
        );
    }
});

app.MapPut("/api/categories/{categoryId}", async (string categoryId, CategoryUpdateDto categoryUpdateDto, CategoryService service) =>
{
    try
    {
        var updatedCategory = await service.UpdateCategoryAsync(categoryId, categoryUpdateDto);
        return Results.Ok(updatedCategory);
    }
    catch (ArgumentException ex)
    {
        return Results.NotFound(new { message = ex.Message });
    }
    catch (Exception ex)
    {
        return Results.Problem(
            statusCode: 500,
            detail: ex.Message,
            title: "Lỗi khi cập nhật danh mục"
        );
    }
});

app.MapDelete("/api/categories/{categoryId}", async (string categoryId, CategoryService service) =>
{
    try
    {
        var result = await service.DeleteCategoryAsync(categoryId);
        if (!result)
        {
            return Results.NotFound(new { message = "Không tìm thấy danh mục" });
        }
        return Results.NoContent();
    }
    catch (ArgumentException ex)
    {
        return Results.NotFound(new { message = ex.Message });
    }
    catch (Exception ex)
    {
        return Results.Problem(
            statusCode: 500,
            detail: ex.Message,
            title: "Lỗi khi xóa danh mục"
        );
    }
});
app.Run();
