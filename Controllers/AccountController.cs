using FluentValidation;
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
        private readonly IEmailService _emailService;
        private readonly ITokenService _tokenService;
        private readonly IUserService _userService;
        private readonly IValidator<LoginViewModel> _loginValidator;
        private readonly IValidator<RegisterViewModel> _registerValidator;
        private readonly IValidator<ResetPasswordViewModel> _resetPasswordValidator;


        public AccountController(IEmailService emailService,
            ITokenService tokenService,
            IUserService userService,
            IInventoryService inventoryService,
            IValidator<LoginViewModel> loginValidator,
            IValidator<RegisterViewModel> registerValidator,
            IValidator<ResetPasswordViewModel> resetPasswordValidator)
        {
            _emailService = emailService;
            _tokenService = tokenService;
            _userService = userService;
            _loginValidator = loginValidator;
            _registerValidator = registerValidator;
            _resetPasswordValidator = resetPasswordValidator;
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
            var user = await _userService.GetUserByEmailAsync(email);
            if (user == null) return NotFound("User not found.");

            var isValid = await _tokenService.IsTokenValidAsync(user.Id, token, TokenType.EmailConfirmation);

            if (!isValid)
                return BadRequest("The confirmation link is invalid or has expired.");

            var emailToken = await _tokenService.GetTokenAsync(user.Id, token, TokenType.EmailConfirmation);
            await _userService.ConfirmEmailAsync(user);

            await _tokenService.InvalidateTokenAsync(emailToken!);

            return RedirectToAction("Login", new { confirmed = true });
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            bool flag = await IsValidModel(_loginValidator, model);
            if (!ModelState.IsValid || !flag) return View(model);
            var user = await _userService.GetUserByEmailAsync(model.Email);
            await SignInUserAsync(user!, model.RememberMe);
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            bool flag = await IsValidModel(_registerValidator, model);
            if (!ModelState.IsValid || !flag) return View(model);

            var user = await _userService.CreateUserAsync(model);

            var token = await _tokenService.GenerateTokenAsync(user.Id, TokenType.EmailConfirmation);
            var confirmationLink = GenerateLink(nameof(ConfirmEmail), new { email = user.Email, token });

            await _emailService.SendEmailConfirmationAsync(user.Email, user.Username, confirmationLink);

            return RedirectToAction("RegistrationConfirmation");
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            bool flag = await IsValidModel(_resetPasswordValidator, model);
            if (!ModelState.IsValid || !flag) return View(model);

            var user = await _userService.GetUserByEmailAsync(model.Email);

            var token = await _tokenService.GenerateTokenAsync(user.Id, TokenType.PasswordReset);
            var resetLink = GenerateLink(nameof(NewPassword), new { userId = user.Id, token });
            await _emailService.SendPasswordResetAsync(user.Email, user.Username, resetLink);

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
            await _userService.ChangePasswordAsync(emailToken!.User, model.Password);
      
            await _tokenService.InvalidateTokenAsync(emailToken);

            TempData["Success"] = "Password successfully changed. You can now log in.";
            return RedirectToAction("Login");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        private string GenerateLink<T>(string action, T routeValues) =>
            Url.Action(action, "Account", routeValues, Request.Scheme)!;

        private async Task SignInUserAsync(User user, bool rememberMe)
        {
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(_userService.GetIdentity(user)),
                new AuthenticationProperties { IsPersistent = rememberMe });
        }

        private async Task<bool> IsValidModel<T>(IValidator<T> contextValidator, T model)
        {
            var validationResult = await contextValidator.ValidateAsync(model);

            if (!validationResult.IsValid)
            {
                foreach (var error in validationResult.Errors)
                {
                    ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                }
                return false;
            }
            return true;
        }
    }
}
