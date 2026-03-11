using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MyProject.Data;
using MyProject.Models.ViewModels.Auth;

namespace MyProject.Validators
{
    public class ResetPasswordValidator : AbstractValidator<ResetPasswordViewModel>
    {
        private readonly ApplicationDbContext _dbContext;

        public ResetPasswordValidator(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;

            VerifyExistsEmail();
        }

        private void VerifyExistsEmail()
        {
            RuleFor(x => x.Email)
                .MustAsync(async (email, cancellation) =>
                    await _dbContext.Users.AnyAsync(u => u.Email == email))
                .WithMessage("If this email exists, a reset link will be sent");
        }

    }
}
