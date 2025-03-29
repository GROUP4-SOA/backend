// Bookstore.API/Middlewares/AuthMiddleware.cs
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Threading.Tasks;

namespace Bookstore.API.Middlewares
{
    public class AuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _jwtSecret = "YourSecretKeyHere1234567890"; // Thay bằng secret key trong appsettings.json

        public AuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            if (string.IsNullOrEmpty(token))
            {
                if (context.Request.Path.StartsWithSegments("/api/auth"))
                {
                    await _next(context);
                    return;
                }

                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Chưa đăng nhập");
                return;
            }

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_jwtSecret);
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                context.Items["User"] = jwtToken.Claims;
            }
            catch
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Token không hợp lệ");
                return;
            }

            await _next(context);
        }
    }
}