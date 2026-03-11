using MyProject.Services.Abstractions;
using System.Text;
using System.Text.Json;

namespace MyProject.Services.Implementions
{
    public class ResendEmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly ILogger<ResendEmailService> _logger;

        public ResendEmailService(IConfiguration configuration, ILogger<ResendEmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClient = new HttpClient();

            SetAuthHeader();
        }

        private void SetAuthHeader()
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _configuration["Resend:ApiKey"]);
        }

        public async Task SendEmailConfirmationAsync(string email, string userName, string confirmationLink)
        {
            var subject = "Подтверждение email";
            var html = $@"
                <h2>Привет, {userName}!</h2>
                <p>Для завершения регистрации подтвердите свой email, перейдя по ссылке:</p>
                <p><a href='{confirmationLink}'>Подтвердить email</a></p>
                <p>Ссылка действительна 24 часа.</p>
                <p>Если вы не регистрировались на нашем сайте, просто проигнорируйте это письмо.</p>";

            await SendEmailAsync(email, subject, html);
        }

        public async Task SendPasswordResetAsync(string email, string userName, string resetLink)
        {
            var subject = "Сброс пароля";
            var html = $@"
                <h2>Привет, {userName}!</h2>
                <p>Вы запросили сброс пароля. Перейдите по ссылке, чтобы установить новый пароль:</p>
                <p><a href='{resetLink}'>Сбросить пароль</a></p>
                <p>Ссылка действительна 1 час.</p>
                <p>Если вы не запрашивали сброс пароля, проигнорируйте это письмо.</p>";

            await SendEmailAsync(email, subject, html);
        }

        private async Task SendEmailAsync(string to, string subject, string htmlBody)
        {
            var response = await PostAsync(SerializePayload(BuildPayload(to, subject, htmlBody)));
            await LogResponseAsync(response);
        }

        private object BuildPayload(string to, string subject, string htmlBody)
        {
            var from = $"{_configuration["Resend:FromName"]} <{_configuration["Resend:FromEmail"]}>";
            return new
            {
                from,
                to = new[] { to },
                subject,
                html = htmlBody
            };
        }

        private StringContent SerializePayload(object payload)
            => new StringContent(JsonSerializer.Serialize(payload), 
                                Encoding.UTF8, 
                                "application/json");
        

        private Task<HttpResponseMessage> PostAsync(HttpContent content)
            => _httpClient.PostAsync("https://api.resend.com/emails", content);
        

        private async Task LogResponseAsync(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Email sent successfully");
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Resend API error: {StatusCode} - {Error}", response.StatusCode, error);
            }
        }
    }
}
