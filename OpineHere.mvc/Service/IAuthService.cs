using OpineHere.mvc.Models;

namespace OpineHere.mvc.Service;

public interface IAuthService
{
    Task<bool> RegisterAsync(RegisterRequest request);
    Task<bool> LoginAsync(LoginRequest request);
    Task LogoutAsync();
    string GetToken();
    string GetUserId();
    string GetUserEmail();
    bool IsAuthenticated();
}
