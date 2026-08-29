using System.Text.Json;
using OpineHere.mvc.Models;

namespace OpineHere.mvc.Service;

// Services/PasetoApiClient.cs
public class PasetoApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PasetoApiClient> _logger;

    public PasetoApiClient(HttpClient httpClient, ILogger<PasetoApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/Identity/register", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<AuthResponse>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError($"Registration failed: {ex.Message}");
            throw;
        }
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/Identity/authenticate", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<AuthResponse>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError($"Login failed: {ex.Message}");
            throw;
        }
    }

    public async Task LogoutAsync(string token)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            await _httpClient.PostAsync("/api/auth/logout", null);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError($"Logout failed: {ex.Message}");
            throw;
        }
    }
    public async Task<(bool Success, string? Message)> ForgotPasswordAsync(
        string email, string resetUrl)
    {
        try
        {
            var payload = new { email, resetUrl };
            var response = await _httpClient.PostAsJsonAsync("/api/Identity/forgot-password", payload);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                return (true, result?.GetValueOrDefault("message"));
            }

            var errorContent = await response.Content
                .ReadFromJsonAsync<Dictionary<string, string>>();
            return (false, errorContent?.GetValueOrDefault("message") ?? "Failed to process request.");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Forgot password error: {ex.Message}");
            return (false, "An error occurred while processing your request.");
        }
    }

    public async Task<(bool Success, string? Message, IEnumerable<string>? Errors)> ResetPasswordAsync(
        string userId, string token, string newPassword, string confirmPassword)
    {
        try
        {
            var payload = new
            {
                userId,
                token,
                newPassword,
                confirmPassword
            };

            var response = await _httpClient.PostAsJsonAsync("/api/Identity/reset-password", payload);

            if (response.IsSuccessStatusCode)
            {
                return (true, "Password has been reset successfully.", null);
            }

            var errorContent = await response.Content
                .ReadFromJsonAsync<Dictionary<string, object>>();

            string message = errorContent?.GetValueOrDefault("message")?.ToString()?? "Password reset failed.";

            List<string>? errors = null;
            if (errorContent?.TryGetValue("errors", out var errorsObj) == true
                && errorsObj is JsonElement jsonElement
                && jsonElement.ValueKind == JsonValueKind.Array){
                errors = jsonElement.EnumerateArray()
                    .Select(e => e.GetString() ?? string.Empty)
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToList();
            }

            return (false, message, errors);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Reset password error: {ex.Message}");
            return (false, "An error occurred while resetting your password.", null);
        }
    }
}


