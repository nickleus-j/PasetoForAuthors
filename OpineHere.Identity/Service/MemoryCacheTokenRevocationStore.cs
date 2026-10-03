using Microsoft.Extensions.Caching.Memory;

namespace OpineHere.Identity.Service;

public class MemoryCacheTokenRevocationStore : ITokenRevocationStore
{
    private readonly IMemoryCache _cache;

    public MemoryCacheTokenRevocationStore(IMemoryCache cache) => _cache = cache;

    public Task<bool> IsTokenUsedAsync(string tokenId)
    {
        return Task.FromResult(_cache.TryGetValue($"used_token:{tokenId}", out _));
    }

    public Task MarkTokenAsUsedAsync(string tokenId, TimeSpan expiration)
    {
        _cache.Set($"used_token:{tokenId}", true, expiration);
        return Task.CompletedTask;
    }
}