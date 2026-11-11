
using System.Net.Http.Headers;
using System.Net.Http.Json;
using PMS.Core.Helpers;
using Microsoft.Extensions.Options;

namespace PMS.Service.Email
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;
        private readonly IHttpClientFactory _httpClientFactory;

        public EmailService(
            IOptions<EmailSettings> settings,
            IHttpClientFactory httpClientFactory)
        {
            _settings = settings.Value;
            _httpClientFactory = httpClientFactory;
        }

        public async Task SendEmailAsync(
            string toEmail,
            string subject,
            string body)
        {
            if (string.IsNullOrWhiteSpace(_settings.ApiKey))
                throw new InvalidOperationException(
                    "Sendliberty API Key is missing.");

            var client = _httpClientFactory.CreateClient();

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer", _settings.ApiKey);

            var request = new
            {
                from = $"{_settings.SenderName} <{_settings.SenderEmail}>",
                to = toEmail,
                subject = subject,
                html = body
            };

            var response = await client.PostAsJsonAsync(
                "https://sendliberty.com/api/send",
                request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();

                throw new HttpRequestException(
                    $"Sendliberty email failed. " +
                    $"Status: {(int)response.StatusCode}. " +
                    $"Details: {error}");
            }
        }
    }
}
