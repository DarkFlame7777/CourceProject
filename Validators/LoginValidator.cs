using FluentValidation;

using Microsoft.EntityFrameworkCore;
using MyProject.Data;
using MyProject.Models.ViewModels.Auth;
using MyProject.Services.Abstractions;


namespace MyProject.Validators
{
    public class LoginValidator : AbstractValidator<LoginViewModel>
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IPasswordHasher _passwordHasher;

        public LoginValidator(ApplicationDbContext dbContext, IPasswordHasher passwordHasher)
        {
            _dbContext = dbContext;
            _passwordHasher = passwordHasher;

            VerifyExistsEmail();
        }

        private void VerifyExistsEmail()
        {
            RuleFor(x => x.Email)
                .MustAsync(async (email, cancellation) =>
                    await _dbContext.Users.AnyAsync(u => u.Email == email))
                .WithMessage("Invalid email or password")
                .DependentRules(() => VerifyConfirmedEmail());
        }
        
        private void VerifyConfirmedEmail()
        {
            RuleFor(x => x.Email)
                .MustAsync(async (email, cancellation) =>
                {
                    var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email, cancellation);
                    return user!.EmailConfirmed;
                })
                .WithMessage("This email is not confirmed").DependentRules(() => VerifyPassword());
        }

        private void VerifyPassword()
        {
            RuleFor(x => x.Password)
                .MustAsync(async (model, password, cancellation) =>
                {
                    var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == model.Email, cancellation);
                    return _passwordHasher.VerifyPassword(password, user!.PasswordHash);
                })
                .WithMessage("Invalid email or password");
        }
    }
}
