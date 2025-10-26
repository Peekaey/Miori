using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ShoukoV2.BusinessService.Interfaces;
using ShoukoV2.Helpers;
using ShoukoV2.Integrations.Anilist.Interfaces;
using ShoukoV2.Models;
using ShoukoV2.Models.Anilist;
using ShoukoV2.Models.Configuration;
using ShoukoV2.Models.Enums;

namespace ShoukoV2.BusinessService;

public class AnilistBusinessService : IAnilistBusinessService
{
    private readonly ILogger<AnilistBusinessService> _logger;
    private readonly IAnilistApiService  _anilistApiService;
    private readonly HybridCache _hybridCache;
    private readonly IConfiguration _configuration;

    public AnilistBusinessService(ILogger<AnilistBusinessService> logger, IAnilistApiService anilistApiService,
        HybridCache hybridCache, IConfiguration configuration)
    {
        _logger = logger;
        _anilistApiService = anilistApiService;
        _hybridCache = hybridCache;
        _configuration = configuration;
    }

    public async Task<Result<AnilistProfileDto>> GetCachedAnilistProfile()
    {
        try
        {
            var enableCaching = _configuration.GetValue<bool>("EnableCaching");

            if (enableCaching == true)
            {
                var cachedData = await _hybridCache.GetOrCreateAsync(
                    CacheKeys.AnilistUser,
                    async cancellationToken =>
                    {
                        _logger.LogApplicationMessage(DateTime.UtcNow, "Cache miss - fetching latest Anilist profile data...");
                        return await FetchAnilistProfileFromApiConcurrently();
                    },
                    new HybridCacheEntryOptions
                    {
                        Expiration = TimeSpan.FromMinutes(30),
                        LocalCacheExpiration = TimeSpan.FromMinutes(30)
                    });
                return Result<AnilistProfileDto>.AsSuccess(cachedData);
            }
            else
            {
                var profileData = await FetchAnilistProfileFromApiConcurrently();
                return Result<AnilistProfileDto>.AsSuccess(profileData);
            }
        }
        catch (Exception ex)
        {
            _logger.LogApplicationException(DateTime.UtcNow, ex, "Error fetching Anilist profile data");
            return Result<AnilistProfileDto>.AsError(ex.Message);
        }
    }
    

    public async Task<AnilistProfileDto> FetchAnilistProfileFromApiConcurrently()
    {
        var anilistProfileDto = new AnilistProfileDto();
        
        var anilistProfileResult = await _anilistApiService.GetAnilistProfileStatistics();

        if (anilistProfileResult.ResultOutcome == ResultEnum.Success)
        {
            anilistProfileDto.AnilistViewerStatistics = anilistProfileResult.Data;
        }
        return anilistProfileDto;
    }
    
    // Legacy Test Call for Debugging
    public async Task<Result<AnilistProfileDto>> GetAnilistProfile()
    {
        try
        {
            AnilistProfileDto anilistProfileDto = new AnilistProfileDto();
            var anilistProfileResult = await _anilistApiService.GetAnilistProfileStatistics();

            if (anilistProfileResult.ResultOutcome == ResultEnum.Success)
            {
                anilistProfileDto.AnilistViewerStatistics = anilistProfileResult.Data;
            }

            return Result<AnilistProfileDto>.AsSuccess(anilistProfileDto);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationException(DateTime.UtcNow, ex, "Error fetching anilist profile data");
            return Result<AnilistProfileDto>.AsError(ex.Message);
        }
    }
    
    
}