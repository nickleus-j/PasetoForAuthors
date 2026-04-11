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
}


