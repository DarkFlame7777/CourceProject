using Microsoft.EntityFrameworkCore;
using MyProject.Data;
using MyProject.Models.Entities;
using MyProject.Services.Abstractions;

namespace MyProject.Services.Implementions
{
    public class TokenService : ITokenService
    {
        private readonly ApplicationDbContext _dbContext;
        private const int EmailConfirmationExpiryHours = 24;
        private const int PasswordResetExpiryHours = 1;

        public TokenService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        private EmailToken CreateToken(int userId, TokenType type, int expiryHours)
            => new EmailToken
            {
                UserId = userId,
                Token = Guid.NewGuid().ToString(),
                Type = type,
                ExpiryDate = DateTime.UtcNow.AddHours(expiryHours),
                IsUsed = false
            };

        private int GetEpiryHours(TokenType type)
            => type switch
            {
                TokenType.EmailConfirmation => EmailConfirmationExpiryHours,
                TokenType.PasswordReset => PasswordResetExpiryHours,
                _ => throw new ArgumentOutOfRangeException(nameof(type))
            };

        public async Task<string> GenerateTokenAsync(int userId, TokenType type)
        {

            await MarkTokensAsUsedAsync(userId, type);

            var token = CreateToken(userId, type, GetEpiryHours(type));
            _dbContext.EmailTokens.Add(token);
            await _dbContext.SaveChangesAsync();

            return token.Token;
        }

        public async Task<bool> IsTokenValidAsync(int userId, string token, TokenType type)
        {
            var emailToken = await GetTokenAsync(userId, token, type);
            return emailToken != null && !emailToken.IsUsed && emailToken.ExpiryDate >= DateTime.UtcNow;
        }

        public async Task<EmailToken?> GetTokenAsync(int userId, string token, TokenType type)
            => await _dbContext.EmailTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.UserId == userId && t.Token == token && t.Type == type);

        public async Task InvalidateTokenAsync(EmailToken token)
        {
            token.IsUsed = true;
            await _dbContext.SaveChangesAsync();
        }

        private async Task MarkTokensAsUsedAsync(int userId, TokenType type)
        {
            var tokens = await _dbContext.EmailTokens
                .Where(t => t.UserId == userId && t.Type == type && !t.IsUsed)
                .ToListAsync();

            foreach (var token in tokens)
                token.IsUsed = true;
        }
    }
}
