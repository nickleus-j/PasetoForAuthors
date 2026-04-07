using OpineHere.Identity.Token;
using Paseto;
using Paseto.Builder;

namespace OpineHere.Identity.Service;

public class PasetoTokenService : ITokenService
{
    private readonly IKeyProvider _keyProvider;
    private readonly ILogger<PasetoTokenService> _logger;
    private readonly IConfiguration _config;

    public PasetoTokenService(
        IKeyProvider keyProvider,
        ILogger<PasetoTokenService> logger,
        IConfiguration config)
    {
        _keyProvider = keyProvider;
        _logger = logger;
        _config = config;
    }

    public string GenerateToken(string userId, string email, 
        TimeSpan? expiration = null)
    {
        try
        {
            var secretKey = _keyProvider.GetSecretKey();
            var now = DateTime.UtcNow;
            var expiresAt = now.Add(
                expiration ?? TimeSpan.FromHours(1));

            var token = new PasetoBuilder()
                .UseV4(Purpose.Public)
                .WithKey(secretKey)
                .AddClaim("sub", userId)
                .AddClaim("email", email)
                .AddClaim("service", "identity-service")
                .Issuer("https://yourdomain.com/identity")
                .Audience("internal-services")
                .Subject(userId)
                .IssuedAt(now)
                .NotBefore(now)
                .Expiration(expiresAt)
                .TokenIdentifier(Guid.NewGuid().ToString())
                .Encode();

            _logger.LogInformation(
                "PASETO token generated for user: {UserId}", userId);
            
            return token;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Failed to generate PASETO token");
            throw;
        }
    }

    public TokenValidationResult ValidateToken(string token)
    {
        try
        {
            var secretKey = _keyProvider.GetSecretKey();

            var validationParams = new PasetoTokenValidationParameters
            {
                ValidateLifetime = true,
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidAudience = "internal-services",
                ValidIssuer = "https://localhost/"
            };

            var result = new PasetoBuilder()
                .UseV4(Purpose.Public)
                .WithKey(secretKey)
                .Decode(token, validationParams);

            if (!result.IsValid)
            {
                _logger.LogWarning("PASETO token validation failed");
                return new TokenValidationResult 
                { 
                    IsValid = false, 
                    ErrorMessage = "Token validation failed" 
                };
            }
            var payload = result.Paseto.Payload;

            // Use the .Get<T> method to retrieve specific claims
            var userId = payload["sub"].ToString();
            var email = payload["email"].ToString();

            return new TokenValidationResult
            {
                IsValid = true,
                UserId = userId,
                Email = email
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "PASETO token validation exception: {Exception}", 
                ex.Message);
            return new TokenValidationResult
            {
                IsValid = false,
                ErrorMessage = ex.Message
            };
        }
    }
}