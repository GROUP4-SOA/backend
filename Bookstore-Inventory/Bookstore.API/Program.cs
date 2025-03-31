using Bookstore.Application.Services;
using Bookstore.Infrastructure.Data;
using Bookstore.Infrastructure.Interfaces.Repositories;
using Bookstore.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using System;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<MongoDbContext>();
builder.Configuration.GetSection("DatabaseSettings");
builder.Services.AddSingleton<IBookRepository, BookRepository>();
builder.Services.AddSingleton<BookService>();

//add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", builder =>
    {
        builder.AllowAnyOrigin();
        builder.AllowAnyMethod();
        builder.AllowAnyHeader();
    });
});

//add logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

//configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseHsts();
}

//API Endpoints
app.MapGet("/api/books", async (BookService service) =>
{
    try
    {
        var books = await service.GetAllBooksAsync();
        return Results.Ok(books);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error retrieving customers: {ex.Message}");
    }
});


app.Run();

