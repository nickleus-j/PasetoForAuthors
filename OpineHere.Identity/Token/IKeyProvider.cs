using Paseto.Cryptography.Key;

namespace OpineHere.Identity.Token;

public interface IKeyProvider
{
    PasetoKey GetSecretKey();
    PasetoKey GetPublicKey();
}