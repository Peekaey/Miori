using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ShoukoV2.BusinessService.Interfaces;
using ShoukoV2.Models;
using ShoukoV2.Models.Enums;

namespace ShoukoV2.BackgroundService;

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
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Background Service is stopping...");
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
                    _logger.LogInformation("Spotify profile data refreshed successfully");
                    return true;
                }
                else
                {
                    _logger.LogError($"Failed to get spotify profile data: {result.ErrorMessage}");
                    return false;
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogError($"Failed to refresh spotify data: {e.Message}");
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
                    _logger.LogInformation("Anilist user data refreshed successfully");
                    return true;
                }
                else
                {
                    _logger.LogError($"Failed to get anilist profile data: {result.ErrorMessage}");
                    return false;
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogError($"Failed to refresh anilist data: {e.Message}");
            return false;
        }
    }
}