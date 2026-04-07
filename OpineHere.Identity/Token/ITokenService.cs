namespace OpineHere.Identity.Token;

public interface ITokenService
{
    string GenerateToken(string userId, string email, 
        TimeSpan? expiration = null);
    TokenValidationResult ValidateToken(string token);
}