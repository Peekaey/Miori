using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Miori.Helpers;
using Miori.Integrations.Spotify.Interfaces;
using Miori.Models;
using Miori.Models.Spotify;

namespace Miori.Cache;

public class SpotifyCacheService : ISpotifyCacheService
{
    private readonly HybridCache _hybridCache;
    private readonly ILogger<SpotifyCacheService> _logger;
    private readonly ISpotifyApiService  _spotifyApiService;
    private readonly IConfiguration _configuration;

    public SpotifyCacheService(HybridCache hybridCache, ILogger<SpotifyCacheService> logger, ISpotifyApiService spotifyApiService, IConfiguration configuration)
    {
        _hybridCache = hybridCache;
        _logger = logger;
        _spotifyApiService = spotifyApiService;
        _configuration = configuration;
    }

    public async Task<Result<SpotifyProfileDto>> GetCachedSpotifyProfile(ulong discordUserId)
    {
        try
        {
            var enableCaching = _configuration.GetValue<bool>("EnableCaching");

            if (enableCaching == true)
            {
                var cachedData = await _hybridCache.GetOrCreateAsync(
                    $"spotify:{discordUserId.ToString()}:profile",
                    async cancellationToken =>
                    {
                        _logger.LogApplicationMessage(DateTime.UtcNow, "Cache miss- fetching latest Spotify profile data...");
                        return await _spotifyApiService.GetSpotifyUserData(discordUserId);
                    }, 
                    new HybridCacheEntryOptions
                    {
                        Expiration = TimeSpan.FromMinutes(15),
                        LocalCacheExpiration = TimeSpan.FromMinutes(5)
                    });
                
                return Result<SpotifyProfileDto>.AsSuccess(cachedData);
            }
            else
            {
                var profileData = await _spotifyApiService.GetSpotifyUserData(discordUserId);
                return Result<SpotifyProfileDto>.AsSuccess(profileData);
            }
        }
        catch (Exception ex)
        {
            _logger.LogApplicationException(DateTime.UtcNow, ex, "Error fetching Spotify profile data in cache layer");
            return Result<SpotifyProfileDto>.AsError(ex.Message);
        }
    }
    
    
}