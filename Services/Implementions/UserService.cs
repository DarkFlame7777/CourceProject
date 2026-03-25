using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using MyProject.Data;
using MyProject.Models.Entities;
using MyProject.Models.ViewModels.Auth;
using MyProject.Services.Abstractions;
using System.Security.Claims;

namespace MyProject.Services.Implementions
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IPasswordHasher _passwordHasher;

        public UserService(ApplicationDbContext dbContext, IPasswordHasher passwordHasher)
        {
            _dbContext = dbContext;
            _passwordHasher = passwordHasher;
        }

        public async Task<User?> GetUserByIdAsync(int id)
            => await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);

        public async Task<User?> GetUserByEmailAsync(string email)
            => await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);

        public async Task<User> CreateUserAsync(RegisterViewModel model)
        {
            var user = CreateModelUser(model.Username, model.Email, model.Password);
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();
            return user;
        }

        private User CreateModelUser(string username, string email, string password)
            => new User
            {
                Username = username,
                Email = email,
                PasswordHash = _passwordHasher.HashPassword(password)
            };

        public async Task ConfirmEmailAsync(User user)
        {
            user.EmailConfirmed = true;
            await _dbContext.SaveChangesAsync();
        }

        public async Task ChangePasswordAsync(User user, string newPassword)
        {
            user.PasswordHash = _passwordHasher.HashPassword(newPassword);
            await _dbContext.SaveChangesAsync();
        }

        private List<Claim> GetUserClaims(User user)
            => new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email)
            };

        public ClaimsIdentity GetIdentity(User user)
            => new ClaimsIdentity(GetUserClaims(user), 
                CookieAuthenticationDefaults.AuthenticationScheme);
    }
}
