using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using Miori.Helpers;
using Miori.Models;
using Miori.Models.Configuration;

namespace Miori.TokenStore;

public class TokenStoreHelpers : ITokenStoreHelpers
{
    private readonly ILogger<TokenStoreHelpers> _logger;
    private readonly HybridCache _hybridCache;

    private readonly HybridCacheEntryOptions _options = new HybridCacheEntryOptions
    {
        Flags = HybridCacheEntryFlags.DisableLocalCacheWrite | HybridCacheEntryFlags.DisableDistributedCacheWrite
    };

    public TokenStoreHelpers(ILogger<TokenStoreHelpers> logger, HybridCache hybridCache)
    {
        _logger = logger;
        _hybridCache = hybridCache;
    }

    public async Task<BasicResult> AddOrUpdateSpotifyToken(ulong discordUserId, SpotifyToken spotifyToken)
    {
        try
        {
            var key = $"spotify:{discordUserId.ToString()}:tokens";

            // First remove existing old key
            await _hybridCache.RemoveAsync(key);

            await _hybridCache.GetOrCreateAsync(
                key,
                async cancellationToken => spotifyToken,
                new HybridCacheEntryOptions
                {
                    Expiration = TimeSpan.FromDays(365), // Need to do research on this
                    LocalCacheExpiration = TimeSpan.FromDays(365),
                }
            );

            _logger.LogApplicationMessage(DateTime.UtcNow, $"Cached spotify tokens for {discordUserId}");
            return BasicResult.AsSuccess();
        }
        catch (Exception ex)
        {
            _logger.LogApplicationException(DateTime.UtcNow, ex, "Failed to cache spotify tokens");
            return BasicResult.AsError(ex.Message);
        }
    }

    public async Task<BasicResult> AddOrUpdateAnilistToken(ulong discordUserId, AnilistToken anilistToken)
    {
        try
        {
            var key = $"anilist:{discordUserId.ToString()}:tokens";

            // First remove existing old key
            await _hybridCache.RemoveAsync(key);

            await _hybridCache.GetOrCreateAsync(
                key,
                async cancellationToken => anilistToken,
                new HybridCacheEntryOptions
                {
                    Expiration = TimeSpan.FromDays(365), // Need to do research on this
                    LocalCacheExpiration = TimeSpan.FromDays(365),
                }
            );

            _logger.LogApplicationMessage(DateTime.UtcNow, $"Cached anilist tokens for {discordUserId}");
            return BasicResult.AsSuccess();
        }
        catch (Exception ex)
        {
            _logger.LogApplicationException(DateTime.UtcNow, ex, "Failed to cache anilist tokens");
            return BasicResult.AsError(ex.Message);
        }
    }
    
    public async Task<BasicResult> AddOrUpdateOsuToken(ulong discordUserId, OsuToken osuToken)
    {
        try
        {
            var key = $"osu:{discordUserId.ToString()}:tokens";

            // First remove existing old key
            await _hybridCache.RemoveAsync(key);

            await _hybridCache.GetOrCreateAsync(
                key,
                async cancellationToken => osuToken,
                new HybridCacheEntryOptions
                {
                    Expiration = TimeSpan.FromHours(23), // Expires in 24 Hours
                    LocalCacheExpiration = TimeSpan.FromHours(23),
                }
            );

            _logger.LogApplicationMessage(DateTime.UtcNow, $"Cached osu tokens for {discordUserId}");
            return BasicResult.AsSuccess();
        }
        catch (Exception ex)
        {
            _logger.LogApplicationException(DateTime.UtcNow, ex, "Failed to cache osu tokens");
            return BasicResult.AsError(ex.Message);
        }
    }
    
    public async Task<BasicResult> RemoveSpotifyToken(ulong discordUserId)
    {
        try
        {
            var key = $"spotify:{discordUserId.ToString()}:tokens";
            await _hybridCache.RemoveAsync(key);
            _logger.LogApplicationMessage(DateTime.UtcNow, $"Removed spotify tokens for {discordUserId}");
            return BasicResult.AsSuccess();
        }
        catch (Exception ex)
        {
            _logger.LogApplicationException(DateTime.UtcNow, ex, "Failed to remove cache for spotify tokens");
            return BasicResult.AsError(ex.Message);
        }
    }

    public async Task<BasicResult> RemoveAnilistToken(ulong discordUserId)
    {
        try
        {
            var key = $"anilist:{discordUserId.ToString()}:tokens";
            await _hybridCache.RemoveAsync(key);
            _logger.LogApplicationMessage(DateTime.UtcNow, $"Removed anilist tokens for {discordUserId}");
            return BasicResult.AsSuccess();
        }
        catch (Exception ex)
        {
            _logger.LogApplicationException(DateTime.UtcNow, ex, "Failed to remove cache for anilist tokens");
            return BasicResult.AsError(ex.Message);
        }
    }
    
    public async Task<BasicResult> RemoveOsuToken(ulong discordUserId)
    {
        try
        {
            var key = $"osu:{discordUserId.ToString()}:tokens";
            await _hybridCache.RemoveAsync(key);
            _logger.LogApplicationMessage(DateTime.UtcNow, $"Removed osu tokens for {discordUserId}");
            return BasicResult.AsSuccess();
        }
        catch (Exception ex)
        {
            _logger.LogApplicationException(DateTime.UtcNow, ex, "Failed to remove cache for osu tokens");
            return BasicResult.AsError(ex.Message);
        }
    }

    public async Task<SpotifyToken?> GetSpotifyTokens(ulong discordUserId)
    {
        try
        {
            var key = $"spotify:{discordUserId.ToString()}:tokens";
            var cachedData = await _hybridCache.GetOrCreateAsync(
                key,
                async cancellationToken => (SpotifyToken?)null,
                new HybridCacheEntryOptions
                {
                    Expiration = TimeSpan.FromSeconds(1),
                    LocalCacheExpiration = TimeSpan.FromSeconds(1),
                });

            if (cachedData == null)
            {
                // Hybrid Cache by default caches null values
                // Immediately remove the cache so we dont cache null values - still a slight delay of caching
                // Not ideal but HybridCache does not expose a read only API
                await _hybridCache.RemoveAsync(key);
                return null;
            }

            return cachedData;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationException(DateTime.UtcNow, ex, $"Exception when attempting to get spotify tokens for user: {discordUserId}");
            return null;
        }
    }
    
    public async Task<AnilistToken?> GetAnilistTokens(ulong discordUserId)
    {
        try
        {
            var key = $"anilist:{discordUserId.ToString()}:tokens";
            var cachedData = await _hybridCache.GetOrCreateAsync(
                key,
                async cancellationToken => (AnilistToken?)null,
                new HybridCacheEntryOptions
                {
                    Expiration = TimeSpan.FromSeconds(1),
                    LocalCacheExpiration = TimeSpan.FromSeconds(1),
                });

            if (cachedData == null)
            {
                // Hybrid Cache by default caches null values
                // Immediately remove the cache so we dont cache null values - still a slight delay of caching
                // Not ideal but HybridCache does not expose a read only API
                await _hybridCache.RemoveAsync(key);
                return null;
            }

            return cachedData;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationException(DateTime.UtcNow, ex, $"Exception when attempting to get anilist tokens for user: {discordUserId}");
            return null;
        }
    }

    public async Task<OsuToken?> GetOsuTokens(ulong discordUserId)
    {
        try
        {
            var key = $"osu:{discordUserId.ToString()}:tokens";
            var cachedData = await _hybridCache.GetOrCreateAsync(
                key,
                async cancellationToken => (OsuToken?)null,
                new HybridCacheEntryOptions
                {
                    Expiration = TimeSpan.FromSeconds(1),
                    LocalCacheExpiration = TimeSpan.FromSeconds(1),
                });

            if (cachedData == null)
            {
                // Hybrid Cache by default caches null values
                // Immediately remove the cache so we dont cache null values - still a slight delay of caching
                // Not ideal but HybridCache does not expose a read only API
                await _hybridCache.RemoveAsync(key);
                return null;
            }
            
            return cachedData;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationException(DateTime.UtcNow, ex, $"Exception when attempting to get osu tokens for user: {discordUserId}");
            return null;
        }
    }
    
    // https://github.com/dotnet/aspnetcore/discussions/57191#discussioncomment-11898121
    public async Task<bool> ExistsAsync(HybridCache cache, string key, CancellationToken token = default)
    {
        (var exists, _) = await TryGetValueAsync<object>(cache, key, token);
        return exists;
    }
    public async Task<(bool, T?)> TryGetValueAsync<T>(HybridCache cache, string key, CancellationToken cancellationToken = default)
    {
        var exists = true;
        var result = await _hybridCache.GetOrCreateAsync<object, T>(
            key,
            null!,
            (_, _) =>
            {
                exists = false;
                return new ValueTask<T>(default(T)!);
            },
            _options,
            null,
            CancellationToken.None);
        
        return (exists, result);
    }
    
}