using Bookstore.Application.Dtos;
using Bookstore.Application.Interfaces.Services;
using Bookstore.API.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Bookstore.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly AuthController _controller; // Add this field

        public AuthControllerTests()
        {
            _authServiceMock = new Mock<IAuthService>();
            _controller = new AuthController(_authServiceMock.Object); // Initialize the controller
        }

        [Fact]
        public async Task Login_ReturnsOk_WithValidCredentials()
        {
            // Arrange
            var loginRequest = new LoginRequestDto { Username = "dinhkhoanam", Password = "hashed_password_10" };
            var user = new UserDto { UserId = "67eacf021af224f20dca68a6", Username = "dinhkhoanam", FullName = "Dinh Khoa Nam", Email = "dinhkhoanam@example.com", PhoneNo = "0910012345", IsActive = true };
            _authServiceMock.Setup(s => s.LoginAsync(loginRequest)).ReturnsAsync(user);

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedUser = Assert.IsType<UserDto>(okResult.Value);
            Assert.Equal(user.UserId, returnedUser.UserId);
            Assert.Equal(user.Username, returnedUser.Username);
            Assert.Equal(user.FullName, returnedUser.FullName);
            Assert.Equal(user.Email, returnedUser.Email);
            Assert.Equal(user.PhoneNo, returnedUser.PhoneNo);
            Assert.Equal(user.IsActive, returnedUser.IsActive);
        }

        [Fact]
        public async Task GetAllUsers_ReturnsOk_WithUsers()
        {
            // Arrange
            var currentUsername = "admin";
            var users = new List<UserDto> { new() { UserId = "67eacf021af224f20dca68a6", Username = "dinhkhoanam", FullName = "Dinh Khoa Nam" } };
            _authServiceMock.Setup(s => s.GetAllUsersAsync(currentUsername)).ReturnsAsync(users);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            _controller.Request.Headers["X-Username"] = currentUsername;

            // Act
            var result = await _controller.GetAllUsers();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedUsers = Assert.IsType<List<UserDto>>(okResult.Value);
            Assert.Equal(users.Count, returnedUsers.Count);
            for (int i = 0; i < users.Count; i++)
            {
                Assert.Equal(users[i].UserId, returnedUsers[i].UserId);
                Assert.Equal(users[i].Username, returnedUsers[i].Username);
                Assert.Equal(users[i].FullName, returnedUsers[i].FullName);
            }
        }

        [Fact]
        public async Task GetAllUsers_ReturnsForbid_WhenUnauthorized()
        {
            // Arrange
            var currentUsername = "user";
            _authServiceMock.Setup(s => s.GetAllUsersAsync(currentUsername))
                .ThrowsAsync(new UnauthorizedAccessException());

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            _controller.Request.Headers["X-Username"] = currentUsername;

            // Act
            var result = await _controller.GetAllUsers();

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task UpdateUser_ReturnsOk_WithValidInput()
        {
            // Arrange
            var userId = "67eacf021af224f20dca689e";
            var currentUserId = "admin";
            var dto = new UpdateUserDto { FullName = "John Updated", Email = "john.updated@example.com", PhoneNo = "12345" };
            var updatedUser = new UserDto { UserId = userId, Username = "tranthimai", FullName = "John Updated", Email = "john.updated@example.com", PhoneNo = "12345" };
            _authServiceMock.Setup(s => s.UpdateUserAsync(userId, dto, currentUserId)).ReturnsAsync(updatedUser);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            _controller.Request.Headers["currentUserId"] = currentUserId;

            // Act
            var result = await _controller.Update(userId, dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedUser = Assert.IsType<UserDto>(okResult.Value);
            Assert.Equal(updatedUser.UserId, returnedUser.UserId);
            Assert.Equal(updatedUser.Username, returnedUser.Username);
            Assert.Equal(updatedUser.FullName, returnedUser.FullName);
            Assert.Equal(updatedUser.Email, returnedUser.Email);
            Assert.Equal(updatedUser.PhoneNo, returnedUser.PhoneNo);
        }


        [Fact]
        public async Task DeactivateUser_ReturnsNoContent_WhenSuccessful()
        {
            // Arrange
            var userId = "67eacf021af224f20dca689e";
            var currentUsername = "admin";

            _authServiceMock.Setup(s => s.DeactivateUserAsync(userId, currentUsername))
                .Returns(Task.CompletedTask);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            _controller.Request.Headers["X-Username"] = currentUsername;

            // Act
            var result = await _controller.DeactivateUser(userId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeactivateUser_ReturnsForbid_WhenUnauthorized()
        {
            // Arrange
            var userId = "67eacf021af224f20dca689e";
            var currentUsername = "user";

            _authServiceMock.Setup(s => s.DeactivateUserAsync(userId, currentUsername))
                .ThrowsAsync(new UnauthorizedAccessException());

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            _controller.Request.Headers["X-Username"] = currentUsername;

            // Act
            var result = await _controller.DeactivateUser(userId);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task DeactivateUser_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = "non-existent-id";
            var currentUsername = "admin";
            var exceptionMessage = "User không tồn tại.";

            _authServiceMock.Setup(s => s.DeactivateUserAsync(userId, currentUsername))
                .ThrowsAsync(new ArgumentException(exceptionMessage));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            _controller.Request.Headers["X-Username"] = currentUsername;

            // Act
            var result = await _controller.DeactivateUser(userId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.NotNull(notFoundResult.Value);

            // Use reflection to access the 'message' property
            var valueType = notFoundResult.Value.GetType();
            var messageProperty = valueType.GetProperty("message");
            Assert.NotNull(messageProperty);

            var actualMessage = messageProperty.GetValue(notFoundResult.Value)?.ToString();
            Assert.Equal(exceptionMessage, actualMessage);
        }
    }
}