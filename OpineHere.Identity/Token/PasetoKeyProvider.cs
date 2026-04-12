using Paseto;
using Paseto.Builder;
using System.Security.Cryptography;
using Paseto.Cryptography.Key;

namespace OpineHere.Identity.Token;

public class PasetoKeyProvider : IKeyProvider
{
    private readonly IConfiguration _config;
    private readonly ILogger<PasetoKeyProvider> _logger;
    private PasetoKey _secretKey,_publicKey;

    public PasetoKeyProvider(
        IConfiguration config,
        ILogger<PasetoKeyProvider> logger)
    {
        _config = config;
        _logger = logger;
        InitializeKey();
    }

    private void InitializeKey()
    {
        var keyString = _config["Paseto:SecretKey"];

        if (!string.IsNullOrEmpty(keyString))
        {
            try
            {
                // Decode the PASERK-encoded key
                _secretKey = Paserk.Decode(keyString);
                _logger.LogInformation(
                    "PASETO signing key loaded from configuration");
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Failed to decode PASETO key from configuration");
            }
        }

        // Generate new key if not configured
        _logger.LogWarning(
            "No PASETO key found. Generating new key pair...");
        GenerateAndLogNewKey();
    }

    private void GenerateAndLogNewKey()
    {
        using (var rng = RandomNumberGenerator.Create())
        {
            // Generate 32-byte seed for Ed25519
            var seed = new byte[32];
            rng.GetBytes(seed);

            // Generate asymmetric key pair (returns AsymmetricKeyPair<PublicKey>)
            var keyPair = new PasetoBuilder()
                .UseV4(Purpose.Public)
                .GenerateAsymmetricKeyPair(seed);

            _secretKey = keyPair.SecretKey;
            _publicKey=keyPair.PublicKey;
            // Encode keys as PASERK for storage
            var secretPaserk = Paserk.Encode(keyPair.SecretKey,PaserkType.Secret);
            var publicPaserk = Paserk.Encode(keyPair.PublicKey,PaserkType.Public);

            _logger.LogCritical(
                "GENERATED NEW PASETO KEY PAIR - Store the secret key in your configuration: {SecretKey}",
                secretPaserk);

            _logger.LogInformation(
                "Public key (share with consuming services): {PublicKey}",
                publicPaserk);
        }
    }

    public PasetoKey GetSecretKey() => _secretKey;

    public PasetoKey GetPublicKey()=> _publicKey;
}