using Microsoft.Extensions.Options;

namespace OpineHere.Identity.Service;

public class MailtrapEmailSender : IEmailSender
{
    private readonly MailtrapSettings _settings;
    private readonly HttpClient _httpClient;
    private readonly ILogger<MailtrapEmailSender> _logger;

    public MailtrapEmailSender(
        IOptions<MailtrapSettings> settings,
        HttpClient httpClient,
        ILogger<MailtrapEmailSender> logger)
    {
        _settings = settings.Value;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "https://send.api.mailtrap.io/api/send")
        {
            Headers =
            {
                { "Authorization", $"Bearer {_settings.ApiKey}" }
            },
            Content = JsonContent.Create(new
            {
                from = new
                {
                    email = _settings.FromEmail,
                    name = _settings.FromName
                },
                to = new[] { new { email } },
                subject,
                html = htmlMessage
            })
        };

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            _logger.LogError($"Mailtrap API error: {response.StatusCode} - {body}");
            throw new Exception($"Failed to send email via Mailtrap API: {response.StatusCode}");
        }

        _logger.LogInformation($"Email sent to {email} via Mailtrap API: {subject}");
    }
}

public class MailtrapSettings
{
    public string ApiKey { get; set; }
    public string FromEmail { get; set; }
    public string FromName { get; set; }
}