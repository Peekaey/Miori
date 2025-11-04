using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Miori.Cache.Interfaces;
using Miori.Helpers;
using Miori.Integrations.Steam;
using Miori.Models;
using Miori.Models.Steam;

namespace Miori.Cache;

public class SteamCacheService : ISteamCacheService
{
    private readonly HybridCache _hybridCache;
    private readonly ILogger<SteamCacheService> _logger;
    private readonly IConfiguration _configuration;
    private readonly ISteamApiService  _steamApiService;

    public SteamCacheService(HybridCache hybridCache, ILogger<SteamCacheService> logger, IConfiguration configuration,
        ISteamApiService steamApiService)
    {
        _hybridCache = hybridCache;
        _logger = logger;
        _configuration = configuration;
        _steamApiService = steamApiService;
    }

    public async Task<Result<SteamApiDto>> GetCachedSteamData(ulong steamId)
    {
        try
        {
            var enableCaching = _configuration.GetValue<bool>("EnableCaching");
            if (enableCaching == true)
            {
                var cachedData = await _hybridCache.GetOrCreateAsync(
                    $"steam:{steamId.ToString()}:profile",
                    async cancellationToken =>
                    {
                        _logger.LogApplicationMessage(DateTime.UtcNow, "Cache miss- fetching latest steam user data");
                        return await _steamApiService.FetchSteamDataFromApiConcurrently(steamId);
                    },
                    new HybridCacheEntryOptions
                    {
                        Expiration = TimeSpan.FromMinutes(15),
                        LocalCacheExpiration = TimeSpan.FromMinutes(5)
                    });
                
                return Result<SteamApiDto>.AsSuccess(cachedData);
            }
            else
            {
                var profileData = await _steamApiService.FetchSteamDataFromApiConcurrently(steamId);
                return Result<SteamApiDto>.AsSuccess(profileData);
            }
        }
        catch (Exception ex)
        {
            _logger.LogApplicationException(DateTime.UtcNow, ex, "Error fetching Steam user data in cache layer");
            return Result<SteamApiDto>.AsError(ex.Message);
        }
    }
    
    public async Task<Result<ulong?>> GetCachedSteamId(string steamId)
    {
        try
        {
            var enableCaching = _configuration.GetValue<bool>("EnableCaching");
            if (enableCaching == true)
            {
                var cachedData = await _hybridCache.GetOrCreateAsync(
                    $"steam:{steamId.ToString()}:profile",
                    async cancellationToken =>
                    {
                        _logger.LogApplicationMessage(DateTime.UtcNow, "Cache miss - fetching latest steam user Id");
                        return await _steamApiService.FetchUniqueSteamId(steamId);
                    },
                    new HybridCacheEntryOptions
                    {
                        Expiration = TimeSpan.FromMinutes(15),
                        LocalCacheExpiration = TimeSpan.FromMinutes(5),
                    });

                return Result<ulong?>.AsSuccess(cachedData);
            }
            else
            {
                var id = await _steamApiService.FetchUniqueSteamId(steamId);
                return Result<ulong?>.AsSuccess(id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogApplicationException(DateTime.UtcNow, ex, "Error fetching Steam user id");
            return Result<ulong?>.AsError(ex.Message);
        }
    }
}