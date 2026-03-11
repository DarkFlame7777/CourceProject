using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyProject.Data;
using MyProject.Models.Entities;
using MyProject.Models.ViewModels.Auth;
using MyProject.Services.Abstractions;
using System.Security.Claims;

namespace MyProject.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IEmailService _emailService;
        private readonly ITokenService _tokenService;
        private readonly IPasswordHasher _passwordHasher;

        public AccountController(
            ApplicationDbContext context,
            IEmailService emailService,
            ITokenService tokenService,
            IPasswordHasher passwordHasher)
        {
            _dbContext = context;
            _emailService = emailService;
            _tokenService = tokenService;
            _passwordHasher = passwordHasher;
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpGet]
        public IActionResult Register() => View();

        [HttpGet]
        public IActionResult ResetPassword() => View();

        [HttpGet]
        public IActionResult RegistrationConfirmation() => View();

        [HttpGet]
        public async Task<IActionResult> NewPassword(int userId, string token)
        {
            var isValid = await _tokenService.IsTokenValidAsync(userId, token, TokenType.PasswordReset);

            if (!isValid)
            {
                TempData["Error"] = "The password reset link is invalid or has expired.";
                return RedirectToAction("Login");
            }

            ViewData["UserId"] = userId;
            ViewData["Token"] = token;

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string email, string token)
        {
            var user = await FindUserbyEmailAsync(email);
            if (user == null)
                return NotFound("User not found.");

            var isValid = await _tokenService.IsTokenValidAsync(user.Id, token, TokenType.EmailConfirmation);

            if (!isValid)
                return BadRequest("The confirmation link is invalid or has expired.");

            var emailToken = await _tokenService.GetTokenAsync(user.Id, token, TokenType.EmailConfirmation);
            user.EmailConfirmed = true;
            await _tokenService.InvalidateTokenAsync(emailToken!);
            await _dbContext.SaveChangesAsync();

            return RedirectToAction("Login", new { confirmed = true });
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await FindUserbyEmailAsync(model.Email);

            await SignInUserAsync(user!, model.RememberMe);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = CreateUser(model.Username, model.Email, model.Password);
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            var token = await _tokenService.GenerateTokenAsync(user.Id, TokenType.EmailConfirmation);
            var confirmationLink = GetConfirmationLink(user.Email, token);

            await _emailService.SendEmailConfirmationAsync(user.Email, user.Username, confirmationLink);

            return RedirectToAction("RegistrationConfirmation");
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await FindUserbyEmailAsync(model.Email);

            if (user != null)
            {
                var token = await _tokenService.GenerateTokenAsync(user.Id, TokenType.PasswordReset);
                var resetLink = GetResetLink(user.Id, token);
                await _emailService.SendPasswordResetAsync(user.Email, user.Username, resetLink);
            }

            TempData["Message"] = "If this email exists, a reset link will be sent.";
            return RedirectToAction("Login");
        }

        [HttpPost]
        public async Task<IActionResult> NewPassword(NewPasswordViewModel model, int userId, string token)
        {
            if (!ModelState.IsValid) return View(model);

            var isValid = await _tokenService.IsTokenValidAsync(userId, token, TokenType.PasswordReset);
            if (!isValid)
            {
                ModelState.AddModelError(string.Empty, "The password reset link is invalid or has expired.");
                return View(model);
            }

            var emailToken = await _tokenService.GetTokenAsync(userId, token, TokenType.PasswordReset);
            emailToken!.User.PasswordHash = _passwordHasher.HashPassword(model.Password);
            await _tokenService.InvalidateTokenAsync(emailToken);
            await _dbContext.SaveChangesAsync();

            TempData["Success"] = "Password successfully changed. You can now log in.";
            return RedirectToAction("Login");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        private async Task<User?> FindUserbyEmailAsync(string email) =>
            await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);

        private User CreateUser(string username, string email, string password) 
            => new User
            {
                Username = username,
                Email = email,
                PasswordHash = _passwordHasher.HashPassword(password)
            };

        private string GetConfirmationLink(string email, string token) =>
            Url.Action("ConfirmEmail", "Account", new { email, token }, Request.Scheme)!;

        private string GetResetLink(int userId, string token) =>
            Url.Action("NewPassword", "Account", new { userId, token }, Request.Scheme)!;

        private async Task SignInUserAsync(User user, bool rememberMe)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                new AuthenticationProperties { IsPersistent = rememberMe });
        }
    }
}
