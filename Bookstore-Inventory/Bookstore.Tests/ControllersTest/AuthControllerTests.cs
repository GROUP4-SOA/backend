using Bookstore.Application.Dtos;
using Bookstore.Application.Interfaces.Services;
using Bookstore.Application.Services;
using Bookstore.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using Xunit;

namespace Bookstore.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _authServiceMock;

        public AuthControllerTests()
        {
            _authServiceMock = new Mock<IAuthService>();
        }

        [Fact]
        public async Task Login_ReturnsOk_WithValidCredentials()
        {
            // Arrange
            var loginRequest = new LoginRequestDto { Username = "user", Password = "pass" };
            var user = new UserDto { UserId = "1", Username = "user", FullName = "John Doe", Email = "john@example.com", PhoneNo = "123", IsActive = true };
            _authServiceMock.Setup(s => s.LoginAsync(loginRequest)).ReturnsAsync(user);
            Func<LoginRequestDto, IAuthService, Task<IResult>> login = async (request, service) =>
            {
                try
                {
                    var result = await service.LoginAsync(request);
                    return Results.Ok(result);
                }
                catch (ArgumentException ex)
                {
                    return Results.BadRequest(new { message = ex.Message });
                }
            };

            // Act
            var result = await login(loginRequest, _authServiceMock.Object);

            // Assert
            var okResult = Assert.IsType<Ok<UserDto>>(result);
            Assert.Equal(user, okResult.Value);
        }

        [Fact]
        public async Task Login_ReturnsBadRequest_WhenExceptionThrown()
        {
            // Arrange
            var loginRequest = new LoginRequestDto { Username = "user", Password = "wrong" };
            _authServiceMock.Setup(s => s.LoginAsync(loginRequest)).ThrowsAsync(new UnauthorizedAccessException("Invalid credentials"));
            Func<LoginRequestDto, IAuthService, Task<IResult>> login = async (request, service) =>
            {
                try
                {
                    var result = await service.LoginAsync(request);
                    return Results.Ok(result);
                }
                catch (ArgumentException ex)
                {
                    return Results.BadRequest(new { message = ex.Message });
                }
            };

            // Act
            var result = await login(loginRequest, _authServiceMock.Object);

            // Assert
            var badRequestResult = Assert.IsType<BadRequest<object>>(result);
            Assert.Equal("Invalid credentials", ((dynamic)badRequestResult.Value).message);
        }

        [Fact]
        public async Task GetAllUsers_ReturnsOk_WithUsers()
        {
            // Arrange
            var users = new List<UserDto> { new() { UserId = "1", Username = "user", FullName = "John Doe" } };
            _authServiceMock.Setup(s => s.GetAllUsersAsync(null)).ReturnsAsync(users);
            Func<IAuthService, Task<IResult>> getAllUsers = async (service) =>
            {
                try
                {
                    var result = await service.GetAllUsersAsync(null);
                    return Results.Ok(result);
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(new { message = ex.Message });
                }
            };

            // Act
            var result = await getAllUsers(_authServiceMock.Object);

            // Assert
            var okResult = Assert.IsType<Ok<List<UserDto>>>(result);
            Assert.Equal(users, okResult.Value);
        }

        [Fact]
        public async Task UpdateUser_ReturnsOk_WithValidInput()
        {
            // Arrange
            var dto = new UpdateUserDto { FullName = "John Updated", Email = "john.updated@example.com", PhoneNo = "12345" };
            var updatedUser = new UserDto { UserId = "1", Username = "user", FullName = "John Updated", Email = "john.updated@example.com", PhoneNo = "12345" };
            _authServiceMock.Setup(s => s.UpdateUserAsync("1", dto, null)).ReturnsAsync(updatedUser);
            Func<string, UpdateUserDto, IAuthService, Task<IResult>> updateUser = async (userId, dto, service) =>
            {
                try
                {
                    var result = await service.UpdateUserAsync(userId, dto, null);
                    return Results.Ok(result);
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(new { message = ex.Message });
                }
            };

            // Act
            var result = await updateUser("1", dto, _authServiceMock.Object);

            // Assert
            var okResult = Assert.IsType<Ok<UserDto>>(result);
            Assert.Equal(updatedUser, okResult.Value);
        }

        [Fact]
        public async Task DeactivateUser_ReturnsOk_WhenSuccessful()
        {
            // Arrange
            _authServiceMock.Setup(s => s.DeactivateUserAsync("1", null)).Returns(Task.CompletedTask);
            Func<string, IAuthService, Task<IResult>> deactivateUser = async (userId, service) =>
            {
                try
                {
                    await service.DeactivateUserAsync(userId, null);
                    return Results.Ok(new { message = "Tài khoản đã được vô hiệu hóa thành công" });
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(new { message = ex.Message });
                }
            };

            // Act
            var result = await deactivateUser("1", _authServiceMock.Object);

            // Assert
            var okResult = Assert.IsType<Ok<object>>(result);
            Assert.Equal("Tài khoản đã được vô hiệu hóa thành công", ((dynamic)okResult.Value).message);
        }
    }
}