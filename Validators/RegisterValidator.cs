using MyProject.Models.ViewModels.Auth;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MyProject.Data;

namespace MyProject.Validators
{
    public class RegisterValidator : AbstractValidator<RegisterViewModel>
    {
        private readonly ApplicationDbContext _dbContext;

        public RegisterValidator(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;

            VerifyUniqueEmail();
        }

        private void VerifyUniqueEmail()
        {
            RuleFor(x => x.Email)
                .MustAsync(async (email, cancellation) =>
                    !await _dbContext.Users.AnyAsync(u => u.Email == email))
                .WithMessage("This email is already registered");
        }
    }
}
