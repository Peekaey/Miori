using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Miori.BusinessService.Interfaces;
using Miori.Helpers;
using Miori.Integrations.Anilist.Interfaces;
using Miori.Models;
using Miori.Models.Anilist;
using Miori.Models.Configuration;
using Miori.Models.Enums;

namespace Miori.BusinessService;

public class AnilistBusinessService : IAnilistBusinessService
{
    private readonly ILogger<AnilistBusinessService> _logger;
    private readonly IAnilistApiService  _anilistApiService;
    private readonly HybridCache _hybridCache;
    private readonly IConfiguration _configuration;
    private readonly AppMemoryStore _appMemoryStore;
    private readonly IAnilistCacheService _anilistCacheService;
    public AnilistBusinessService(ILogger<AnilistBusinessService> logger, IAnilistApiService anilistApiService,
        HybridCache hybridCache, IConfiguration configuration, AppMemoryStore appMemoryStore, IAnilistCacheService anilistCacheService)
    {
        _logger = logger;
        _anilistApiService = anilistApiService;
        _hybridCache = hybridCache;
        _configuration = configuration;
        _appMemoryStore = appMemoryStore;
        _anilistCacheService = anilistCacheService;
    }
    
    public async Task<BasicResult> RegisterNewAnilistUser(ulong discordUserId, AnilistTokenResponse tokenResponse)
    {
        var newProfileResponse = await  _anilistApiService.GetAnilistProfileIdForNewRegister(tokenResponse);

        if (newProfileResponse.ResultOutcome != ResultEnum.Success)
        {
            _logger.LogApplicationError(DateTime.UtcNow, $"Error getting anilist profile for discord user {discordUserId} with error: {newProfileResponse.ErrorMessage}");
        }
        
        _logger.LogApplicationMessage(DateTime.UtcNow, $"Successfully tied Discord User Id : '{discordUserId}' to Anilist User : '{newProfileResponse.Data}'");
        _appMemoryStore.AddOrUpdateAnilistToken(discordUserId, AnilistToken.Create(discordUserId, newProfileResponse.Data.ToString(), tokenResponse.access_token, tokenResponse.refresh_token));
        return BasicResult.AsSuccess();
    }

    public async Task<Result<AnilistProfileDto>> GetAnilistProfileForApi(ulong discordUserId)
    {
        var isAnilistFound = _appMemoryStore.TryGetAnilistToken(discordUserId, out var anilistToken);

        if (isAnilistFound == false)
        {
            return Result<AnilistProfileDto>.AsFailure("Anilist user registered to the provider discord user Id does not exist");
        }

        var profileResult = await _anilistCacheService.GetCachedAnilistProfile(discordUserId);

        if (profileResult.ResultOutcome != ResultEnum.Success)
        {
            return Result<AnilistProfileDto>.AsFailure(profileResult.ErrorMessage);
        }
        
        return Result<AnilistProfileDto>.AsSuccess(profileResult.Data);
        
    }
    
    
}