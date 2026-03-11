namespace MyProject.Services.Abstractions
{
    public interface IEmailService
    {
        Task SendEmailConfirmationAsync(string email, string userName, string confirmationLink);
        Task SendPasswordResetAsync(string email, string userName, string resetLink);
    }
}
