using OpineHere.Identity.Token;
using Paseto;
using Paseto.Builder;
using Paseto.Cryptography.Key;

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
                .AddClaim("uid", userId)
                .AddClaim("email", email)
                .AddClaim("service", "identity-service")
                .Issuer(_config["Issuer"])
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
        // 1. Get the key from your provider
        var key = _keyProvider.GetPublicKey();

        // 2. Identify the correct Verification Key
        PasetoKey verificationKey;

        if (key is PasetoAsymmetricSecretKey secretKey)
        {
            // IMPORTANT: We do not CAST. We EXTRACT the public part.
            verificationKey = secretKey; 
        }
        else if (key is PasetoAsymmetricPublicKey publicKey)
        {
            // If it's already a public key, we're good to go
            verificationKey = publicKey;
        }
        else
        {
            throw new InvalidOperationException(
                $"Key type {key.GetType().Name} is not supported for V4 Public validation.");
        }

        // 3. Configure validation parameters
        var validationParams = new PasetoTokenValidationParameters
        {
            ValidateLifetime = true,
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidAudience = "internal-services",
            ValidIssuer = _config["Issuer"],
        };

        // 4. Decode using the Verification (Public) Key
        var result = new PasetoBuilder()
            .UseV4(Purpose.Public)
            .WithKey(verificationKey) 
            .Decode(token, validationParams);

        if (!result.IsValid)
        {
            // Access the internal exception message to see why it failed
            return new TokenValidationResult 
            { 
                IsValid = false, 
                ErrorMessage = result.Exception?.Message ?? "Validation failed" 
            };
        }

        return new TokenValidationResult
        {
            IsValid = true,
            UserId = result.Paseto.Payload["sub"]?.ToString(),
            Email = result.Paseto.Payload["email"]?.ToString()
        };
    }
    catch (Exception ex)
    {
        // This is where your "Unable to cast" error was being caught
        return new TokenValidationResult { IsValid = false, ErrorMessage = ex.Message };
    }
}
}