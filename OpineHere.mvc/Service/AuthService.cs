using OpineHere.mvc.Models;

namespace OpineHere.mvc.Service;

public class AuthService : IAuthService
{
    private readonly PasetoApiClient _apiClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuthService> _logger;
    private const string TokenCookieName = "paseto_token";
    private const string RefreshTokenCookieName = "refresh_token";
    private const string UserIdCookieName = "user_id";
    private const string UserEmailCookieName = "user_email";

    public AuthService(PasetoApiClient apiClient, IHttpContextAccessor httpContextAccessor, 
        ILogger<AuthService> logger)
    {
        _apiClient = apiClient;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<bool> LoginAsync(LoginRequest request)
    {
        try
        {
            var response = await _apiClient.LoginAsync(request);
            StoreCookies(response);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Login error: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> RegisterAsync(RegisterRequest request)
    {
        try
        {
            var response = await _apiClient.RegisterAsync(request);
            StoreCookies(response);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Registration error: {ex.Message}");
            return false;
        }
    }

    public string GetToken()
    {
        string? token=null;
        _httpContextAccessor.HttpContext?.Request.Cookies
            .TryGetValue(TokenCookieName, out token);
        return token;
    }

    public string GetUserId()
    {
        string? userId=string.Empty;
        _httpContextAccessor.HttpContext?.Request.Cookies
            .TryGetValue(UserIdCookieName, out userId);
        return userId;
    }

    public string GetUserEmail()
    {
        string? email=string.Empty;
        _httpContextAccessor.HttpContext?.Request.Cookies
            .TryGetValue(UserEmailCookieName, out email);
        return email;
    }

    public bool IsAuthenticated()
    {
        return !string.IsNullOrEmpty(GetToken());
    }

    private void StoreCookies(AuthResponse response)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.Now.AddHours(1)
        };

        var context = _httpContextAccessor.HttpContext;
        context?.Response.Cookies.Append(TokenCookieName, response.Token, cookieOptions);
        context?.Response.Cookies.Append(UserIdCookieName, response.UserId, cookieOptions);
        if (!string.IsNullOrEmpty(response.UserEmail))
        {
            context?.Response.Cookies.Append(UserEmailCookieName, response.UserEmail, cookieOptions);
        }
        
        if (!string.IsNullOrEmpty(response.RefreshToken))
        {
            context?.Response.Cookies.Append(RefreshTokenCookieName, response.RefreshToken, cookieOptions);
        }
    }

    public async Task LogoutAsync()
    {
        var token = GetToken();
        if (token != null)
        {
            try
            {
                await _apiClient.LogoutAsync(token);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Logout error: {ex.Message}");
            }
        }
        ClearCookies();
    }

    private void ClearCookies()
    {
        var context = _httpContextAccessor.HttpContext;
        context?.Response.Cookies.Delete(TokenCookieName);
        context?.Response.Cookies.Delete(RefreshTokenCookieName);
        context?.Response.Cookies.Delete(UserIdCookieName);
        context?.Response.Cookies.Delete(UserEmailCookieName);
    }
}