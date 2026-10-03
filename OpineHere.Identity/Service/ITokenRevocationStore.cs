namespace OpineHere.Identity.Service;

public interface ITokenRevocationStore
{
    Task<bool> IsTokenUsedAsync(string tokenId);
    Task MarkTokenAsUsedAsync(string tokenId, TimeSpan expiration);
}

