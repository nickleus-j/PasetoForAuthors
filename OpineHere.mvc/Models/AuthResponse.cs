namespace OpineHere.mvc.Models;

public class AuthResponse
{
    public string Token { get; set; }
    public string RefreshToken { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string UserId { get; set; }
    public string UserEmail { get; set; }
    public string UserName { get; set; }
}