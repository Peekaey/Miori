using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Miori.BusinessService;
using Miori.Helpers;
using Miori.Integrations.Anilist.Interfaces;
using Miori.Models;
using Miori.Models.Anilist;

namespace Miori.Cache;

public class AnilistCacheService : IAnilistCacheService
{
    private readonly HybridCache _hybridCache;
    private readonly ILogger<AnilistCacheService> _logger;
    private readonly IAnilistApiService  _anilistApiService;
    private readonly IConfiguration _configuration;

    public AnilistCacheService(HybridCache hybridCache, ILogger<AnilistCacheService> logger,
        IAnilistApiService anilistApiService, IConfiguration configuration)
    {
        _hybridCache = hybridCache;
        _logger = logger;
        _anilistApiService = anilistApiService;
        _configuration = configuration;
    }

    public async Task<Result<AnilistProfileDto>> GetCachedAnilistProfile(ulong discordUserId)
    {
        try
        {
            var enableCaching = _configuration.GetValue<bool>("EnableCaching");

            if (enableCaching == true)
            {
                var cachedData = await _hybridCache.GetOrCreateAsync(
                    $"anilist:{discordUserId.ToString()}:profile",
                    async cancellationToken =>
                    {
                        _logger.LogApplicationMessage(DateTime.UtcNow,
                            "Cache miss - fetching latest Anilist profile data...");
                        return await _anilistApiService.GetAnilistUserData(discordUserId);
                    },
                    new HybridCacheEntryOptions
                    {
                        Expiration = TimeSpan.FromMinutes(15),
                        LocalCacheExpiration = TimeSpan.FromMinutes(15)
                    });

                return Result<AnilistProfileDto>.AsSuccess(cachedData);

            }
            else
            {
                var profileData = await  _anilistApiService.GetAnilistUserData(discordUserId);
                return Result<AnilistProfileDto>.AsSuccess(profileData);
            }
        }
        catch (Exception ex)
        {
            _logger.LogApplicationException(DateTime.UtcNow, ex, "Error fetching Anilist profile data in cache layer");
            return Result<AnilistProfileDto>.AsError(ex.Message);
        }
    }
}