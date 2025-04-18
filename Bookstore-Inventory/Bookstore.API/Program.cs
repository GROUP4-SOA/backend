﻿using Bookstore.Application.Dtos;
using Bookstore.Application.Interfaces.Services;
using Bookstore.Application.Services;
using Bookstore.Infrastructure.Data;
using Bookstore.Infrastructure.Interfaces.Repositories;
using Bookstore.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;

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
builder.Services.AddSingleton<IWarehouseExportRepository, WarehouseExportRepository>();
builder.Services.AddSingleton<ExportService>();
builder.Services.AddSingleton<IImportService, ImportService>();  // Import Service
builder.Services.AddSingleton<IAuthService, AuthService>();
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

// Add Controllers

var app = builder.Build();

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseHsts();
}

app.UseCors("CorsPolicy");

// Enable Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Bookstore API V1");
    c.RoutePrefix = "swagger";
});

// API Endpoints
app.MapGet("/api/books", async (BookService service) =>
{
    var books = await service.GetAllBooksAsync();
    return Results.Ok(books);
});

app.MapGet("/api/books/{bookId}", async (string bookId, BookService service) =>
{
    var book = await service.GetBookByIdAsync(bookId);
    return Results.Ok(book);
});

app.MapPost("/api/books", async (BookCreateDto bookCreateDto, BookService service) =>
{
    var createdBook = await service.CreateBookAsync(bookCreateDto);
    return Results.Created($"/api/books/{createdBook.BookId}", createdBook);
});

app.MapPut("/api/books/{bookId}", async (string bookId, BookUpdateDto bookUpdateDto, BookService service) =>
{
    var updatedBook = await service.UpdateBookAsync(bookId, bookUpdateDto);
    return Results.Ok(updatedBook);
});

app.MapDelete("/api/books/{bookId}", async (string bookId, BookService service) =>
{
    var result = await service.DeleteBookAsync(bookId);
    return result ? Results.NoContent() : Results.NotFound();
});
app.MapGet("/api/books/category/{categoryId}", async (string categoryId, BookService bookService) =>
{
    try
    {
        // Lấy danh sách sách theo categoryId
        var books = await bookService.GetBooksByCategoryAsync(categoryId);
        return Results.Ok(books); // Trả về danh sách sách theo categoryId
    }
    catch (Exception ex)
    {
        // Nếu có lỗi, trả về lỗi BadRequest
        return Results.BadRequest(new { message = ex.Message });
    }
});

// Category
app.MapGet("/api/categories", async (CategoryService service) =>
{
    var categories = await service.GetAllCategoriesAsync();
    return Results.Ok(categories);
});

app.MapGet("/api/categories/{categoryId}", async (string categoryId, CategoryService service) =>
{
    var category = await service.GetCategoryByIdAsync(categoryId);
    return Results.Ok(category);
});

app.MapPost("/api/categories", async (CategoryCreateDto categoryCreateDto, CategoryService service) =>
{
    var createdCategory = await service.CreateCategoryAsync(categoryCreateDto);
    return Results.Created($"/api/categories/{createdCategory.CategoryId}", createdCategory);
});

app.MapPut("/api/categories/{categoryId}", async (string categoryId, CategoryUpdateDto categoryUpdateDto, CategoryService service) =>
{
    var updatedCategory = await service.UpdateCategoryAsync(categoryId, categoryUpdateDto);
    return Results.Ok(updatedCategory);
});

app.MapDelete("/api/categories/{categoryId}", async (string categoryId, CategoryService service) =>
{
    var result = await service.DeleteCategoryAsync(categoryId);
    return result ? Results.NoContent() : Results.NotFound();
});

// ===== AUTH ENDPOINTS =====
app.MapPost("/api/auth/login", async (LoginRequestDto loginRequest, IAuthService authService) =>
{
    try
    {
        var user = await authService.LoginAsync(loginRequest);
        return Results.Ok(user);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
});
// Endpoint để tạo user mới
app.MapPost("/api/users", async (CreateUserDto newUserDto, IAuthService authService) =>
{
    try
    {
        var createdUser = await authService.CreateUserAsync(newUserDto);
        return Results.Created($"/users/{createdUser.UserId}", createdUser); // Trả về status 201 với đường dẫn của user vừa tạo
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex.Message); // Trả về lỗi nếu có lỗi
    }
});

app.MapGet("/api/auth/users", async (IAuthService authService) =>
{
    try
    {
        var users = await authService.GetAllUsersAsync(null);
        return Results.Ok(users);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
});

app.MapPut("/api/auth/{userId}", async (string userId, UpdateUserDto updateUser, IAuthService authService) =>
{
    try
    {
        var updatedUser = await authService.UpdateUserAsync(userId, updateUser, null);
        return Results.Ok(updatedUser);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
});

app.MapPut("/api/auth/deactivate/{userId}", async (string userId, IAuthService authService) =>
{
    try
    {
        await authService.DeactivateUserAsync(userId, null);
        return Results.Ok(new { message = "Tài khoản đã được vô hiệu hóa thành công" });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
});

// Warehouse Export
app.MapGet("/api/warehouse-exports", async (ExportService service) =>
{
    var exports = await service.GetAllExportsAsync();
    return Results.Ok(exports);
});

app.MapPost("/api/warehouse-exports", async (WarehouseExportDto exportDto, ExportService service) =>
{
    var createdExport = await service.CreateExportAsync(exportDto);
    return Results.Created($"/api/warehouse-exports/{createdExport.ExportId}", createdExport);
});
// ===== IMPORT ENDPOINTS =====
app.MapPost("/api/imports", async (WarehouseImportDto importDto, IImportService importService) =>
{
    try
    {
        var createdImport = await importService.CreateImportAsync(importDto);
        return Results.Created($"/api/imports/{createdImport.ImportId}", createdImport);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
});

app.MapGet("/api/imports", async (IImportService importService) =>
{
    var imports = await importService.GetAllImportsAsync();
    return Results.Ok(imports);
});

app.Run();
