using Paseto;
using Paseto.Builder;
using Paseto.Cryptography;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using OpineHere.Identity.Token;

namespace OpineHere.Identity.Service;

public class PasetoTokenMakerService:IPasetoTokenMakerService
{
    private readonly byte[] _privateKeyBytes;
    private readonly byte[] _publicKeyBytes;
    private const string Issuer = "OpineHere.Identity";
    private const string Audience = "OpineHere.mvc";

    public PasetoTokenMakerService(IConfiguration config,IKeyProvider keyProvider)
    {
        _privateKeyBytes = keyProvider.GetSecretKey().Key.ToArray();
        _publicKeyBytes = keyProvider.GetPublicKey().Key.ToArray();
    }

    public string GenerateMagicLinkToken(string authorId, string email)
    {
        var now = DateTime.UtcNow;

        return new PasetoBuilder()
            .UseV4(Purpose.Public)
            .WithSecretKey(_privateKeyBytes)
            .Issuer(Issuer)
            .Audience(Audience)
            .Subject(authorId)
            .Expiration(now.AddMinutes(15)) // Short lifespan
            .IssuedAt(now)
            .AddClaim("email", email)
            .AddClaim("jti", Guid.NewGuid().ToString()) // Unique token ID for replay prevention
            .AddClaim("purpose", "passwordless-login")
            .Encode();
    }

    public PasetoTokenValidationResult ValidateMagicLinkToken(string token)
    {
        try
        {
            var valParams = new PasetoTokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidIssuer = Issuer,
                ValidateAudience = true,
                ValidAudience = Audience,
                ValidateLifetime = true
            };

            var result = new PasetoBuilder()
                .UseV4(Purpose.Public)
                .WithPublicKey(_publicKeyBytes)
                .Decode(token, valParams);

            var payload = result.Paseto.Payload;

            if (payload["purpose"]?.ToString() != "passwordless-login")
            {
                return new PasetoTokenValidationResult(false, null, null, null, "Invalid token purpose.");
            }

            return new PasetoTokenValidationResult(
                IsValid: true,
                AuthorId: payload["sub"]?.ToString(),
                Email: payload["email"]?.ToString(),
                TokenId: payload["jti"]?.ToString(),
                ErrorMessage: null
            );
        }
        catch (Exception ex)
        {
            return new PasetoTokenValidationResult(false, null, null, null, ex.Message);
        }
    }
}