namespace OpineHere.Identity.Authentication;

public class AuthenticateRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
}

public class TokenValidationRequest
{
    public string Token { get; set; }
}

public class RefreshTokenRequest
{
    public string Token { get; set; }
}