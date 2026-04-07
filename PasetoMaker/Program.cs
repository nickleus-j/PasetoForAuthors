using Paseto;
using Paseto.Builder;
using System.Security.Cryptography;using Org.BouncyCastle.Crypto;
using Paseto.Cryptography.Key;
using Paseto.Protocol;

byte[] makeRandomBytes()
{
    var keyBytes = new byte[32];
    RandomNumberGenerator.Fill(keyBytes);
    return keyBytes;
}
PasetoKey KeyGen()
{
    // 1. Generate 32 raw random bytes
    var keyBytes = makeRandomBytes();

    // 2. Wrap it in the appropriate Paseto Key object
    return new PasetoSymmetricKey(keyBytes, new Version4());
}

Console.WriteLine("Hello, World!");
// In a setup/admin endpoint or console app

var pasetoKey = KeyGen();

// 3. Manually encode to PASERK if needed (requires Paserk class)
var paserkString = Paserk.Encode(pasetoKey, PaserkType.Local); 

Console.WriteLine($"Paserk Local: {paserkString}");
var builder =new PasetoBuilder();
var localToken = builder
    .UseV4(Purpose.Local)
    .WithKey(pasetoKey)
    .AddClaim("user_id", 123)
    .Expiration(DateTime.UtcNow.AddHours(1))
    .Encode();
Console.WriteLine($"Local Token: {localToken}");

var publicKey = builder
    .UseV4(Purpose.Public)
    .GenerateAsymmetricKeyPair(makeRandomBytes());

// Store the secret key securely 
Console.WriteLine($"Secret (store securely): {Paserk.Encode(publicKey.SecretKey,PaserkType.Secret)}");
Console.WriteLine($"Public (share with services): {Paserk.Encode(publicKey.PublicKey,PaserkType.Public)}");