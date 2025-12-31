using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Miori.Cache.Interfaces;
using Miori.Helpers;
using Miori.Integrations.Osu;
using Miori.Models;
using Miori.Models.Osu;

namespace Miori.Cache;

public class OsuCacheService : IOsuCacheService
{
    private readonly HybridCache _hybridCache;
    private readonly ILogger<OsuCacheService> _logger;
    private readonly IOsuApiService _osuApiService;
    private readonly IConfiguration _configuration;

    public OsuCacheService(HybridCache hybridCache, ILogger<OsuCacheService> logger,
        IOsuApiService osuApiService, IConfiguration configuration)
    {
        _hybridCache = hybridCache;
        _logger = logger;
        _osuApiService = osuApiService;
        _configuration = configuration;
    }

    public async Task<Result<OsuProfileDto>> GetCachedOsuProfile(ulong discordUserId)
    {
        try
        {
            var enableCaching = _configuration.GetValue<bool>("EnableCaching");

            if (enableCaching == true)
            {
                var cachedData = await _hybridCache.GetOrCreateAsync(
                    $"osu:{discordUserId.ToString()}:profile",
                    async cancellationToken =>
                    {
                        _logger.LogApplicationMessage(DateTime.UtcNow, "Cache miss - fetching lastest Osu profile data...");
                        return await _osuApiService.GetOsuUserData(discordUserId);
                    },
                    new HybridCacheEntryOptions
                    {
                        Expiration = TimeSpan.FromMinutes(15),
                        LocalCacheExpiration = TimeSpan.FromMinutes(5)
                    });

                return Result<OsuProfileDto>.AsSuccess(cachedData);
            }
            else
            {
                var profileData = await _osuApiService.GetOsuUserData(discordUserId);
                return Result<OsuProfileDto>.AsSuccess(profileData);
            }
        }
        catch (Exception ex)
        {
            _logger.LogApplicationException(DateTime.UtcNow, ex, "Error fetching Osu profile data in cache layer");
            return Result<OsuProfileDto>.AsError(ex.Message);
        }
    }
}