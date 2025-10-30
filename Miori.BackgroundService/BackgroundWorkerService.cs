using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Miori.BusinessService.Interfaces;
using Miori.Helpers;
using Miori.Models;
using Miori.Models.Enums;

namespace Miori.BackgroundService;

public class BackgroundWorkerService : Microsoft.Extensions.Hosting.BackgroundService , IBackgroundWorkerService
{
    private readonly ILogger<BackgroundWorkerService> _logger;
    private readonly IServiceScopeFactory  _scopeFactory;
    private readonly TimeProvider _timeProvider;
    private readonly HybridCache _hybridCache;
    private readonly IConfiguration _configuration;
    
    public BackgroundWorkerService(ILogger<BackgroundWorkerService> logger, IServiceScopeFactory scopeFactory, TimeProvider timeProvider,
        HybridCache hybridCache, IConfiguration configuration)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _timeProvider = timeProvider;
        _hybridCache = hybridCache;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Background Service is starting...");
        await RefreshAllCachesAsync();

        using (var timer = new PeriodicTimer(TimeSpan.FromMinutes(30)))
        {
            try
            {
                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    await RefreshAllCachesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogApplicationException(DateTime.UtcNow, ex, "Exception from Background Worker Service");
                _logger.LogCritical("Background Service is stopping...");
            }
        }
    }

    public async Task RefreshAllCachesAsync()
    {
        var tasks = new[]
        {
            RefreshSpotifyDataCache(),
            RefreshAnilistDataCache()
        };

        var results = await Task.WhenAll(tasks);

        var successCount = results.Count(r => r);
        var totalCount = results.Length;
        
        _logger.LogInformation("Cache refresh completed: {Success}/{Total} services updated", 
            successCount, totalCount);
    }

    public async Task<bool> RefreshSpotifyDataCache()
    {
        try
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var spotifyBusinessService = scope.ServiceProvider.GetRequiredService<ISpotifyBusinessService>();
                var result = await spotifyBusinessService.GetSpotifyProfile();
                
                if (result.ResultOutcome == ResultEnum.Success)
                {
                    await _hybridCache.RemoveAsync(CacheKeys.SpotifyProfile);
                    
                    await _hybridCache.SetAsync(
                        CacheKeys.SpotifyProfile,
                        result.Data,
                        new HybridCacheEntryOptions
                        {
                            Expiration = TimeSpan.FromMinutes(30),
                            LocalCacheExpiration = TimeSpan.FromMinutes(30)
                        });
                    _logger.LogApplicationMessage(DateTime.UtcNow, "Spotify profile data refreshed successfully");
                    return true;
                }
                else
                {
                    string errorMessage = $"Failed to get Spotify profile data for BackgroundWorkerService: {result.ErrorMessage}";
                    _logger.LogApplicationMessage(DateTime.UtcNow, "Spotify profile data refresh failed");
                    return false;
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogApplicationException(DateTime.UtcNow, e,
                "Failed to refresh Spotify profile data in BackgroundWorkerService");
            return false;
        }
    }

    public async Task<bool> RefreshAnilistDataCache()
    {
        try
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var anilistBusinessService = scope.ServiceProvider.GetRequiredService<IAnilistBusinessService>();
                var result = await anilistBusinessService.GetAnilistProfile();

                if (result.ResultOutcome == ResultEnum.Success)
                {
                    await _hybridCache.RemoveAsync(CacheKeys.AnilistUser);
                    await _hybridCache.SetAsync(
                        CacheKeys.AnilistUser,
                        result.Data,
                        new HybridCacheEntryOptions
                        {
                            Expiration = TimeSpan.FromMinutes(30), // Remote Node
                            LocalCacheExpiration = TimeSpan.FromMinutes(30) // Inbuilt Memory
                        });
                    _logger.LogApplicationMessage(DateTime.UtcNow, "Anilist user data refreshed successfully");
                    return true;
                }
                else
                {
                    string errorMessage = $"Failed to get Anilist profile data for BackgroundWorkerService: {result.ErrorMessage}";
                    _logger.LogApplicationError(DateTime.UtcNow, errorMessage);
                    return false;
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogApplicationException(DateTime.UtcNow, e,"Failed to refresh Anilist profile data cache in BackgroundWorkerService");
            return false;
        }
    }
}