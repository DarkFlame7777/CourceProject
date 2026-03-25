using MyProject.Models.Entities;
using MyProject.Models.ViewModels.Auth;
using System.Security.Claims;

namespace MyProject.Services.Abstractions
{
    public interface IUserService
    {
        Task<User?> GetUserByIdAsync(int id);
        Task<User?> GetUserByEmailAsync(string email);

        Task<User> CreateUserAsync(RegisterViewModel model);
        Task ConfirmEmailAsync(User user);
        Task ChangePasswordAsync(User user, string newPassword);

        ClaimsIdentity GetIdentity(User user);
    }
}
