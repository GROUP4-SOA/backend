// Bookstore.API/Middlewares/ExceptionMiddleware.cs
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace Bookstore.API.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(new
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra: " + ex.Message
                }.ToString());
            }
        }
    }
}