﻿using Bookstore.Application.Dtos;
using System.Threading.Tasks;

namespace Bookstore.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<UserDto> LoginAsync(LoginRequestDto loginRequest);
        Task<UserDto> UpdateUserAsync(string userId, UpdateUserDto updateUser, string currentUsername);
        Task DeleteUserAsync(string userId, string currentUsername);
    }
}