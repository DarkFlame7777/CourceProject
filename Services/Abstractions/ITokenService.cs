using MyProject.Models.Entities;

namespace MyProject.Services.Abstractions
{
    public interface ITokenService
    {
        Task<string> GenerateTokenAsync(int userId, TokenType type);
        Task<bool> IsTokenValidAsync(int userId, string token, TokenType type);
        Task<EmailToken?> GetTokenAsync(int userId, string token, TokenType type);
        Task InvalidateTokenAsync(EmailToken token);
    }
}
