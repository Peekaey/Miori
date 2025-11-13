using System.Net;
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
using Miori.TokenStore;

namespace Miori.BusinessService;

public class AnilistBusinessService : IAnilistBusinessService
{
    private readonly ILogger<AnilistBusinessService> _logger;
    private readonly IAnilistApiService  _anilistApiService;
    private readonly HybridCache _hybridCache;
    private readonly IConfiguration _configuration;
    private readonly IAnilistCacheService _anilistCacheService;
    private readonly ITokenStoreHelpers _tokenStoreHelper;
    public AnilistBusinessService(ILogger<AnilistBusinessService> logger, IAnilistApiService anilistApiService,
        HybridCache hybridCache, IConfiguration configuration, IAnilistCacheService anilistCacheService,
        ITokenStoreHelpers tokenStoreHelper)
    {
        _logger = logger;
        _anilistApiService = anilistApiService;
        _hybridCache = hybridCache;
        _anilistCacheService = anilistCacheService;
        _tokenStoreHelper = tokenStoreHelper;
    }
    
    public async Task<BasicResult> RegisterNewAnilistUser(ulong discordUserId, AnilistTokenResponse tokenResponse)
    {
        var newProfileResponse = await  _anilistApiService.GetAnilistProfileIdForNewRegister(tokenResponse);

        if (newProfileResponse.ResultOutcome != ResultEnum.Success)
        {
            _logger.LogApplicationError(DateTime.UtcNow, $"Error getting anilist profile for discord user {discordUserId} with error: {newProfileResponse.ErrorMessage}");
        }
        
        _logger.LogApplicationMessage(DateTime.UtcNow, $"Successfully tied Discord User Id : '{discordUserId}' to Anilist User : '{newProfileResponse.Data}'");
        await _tokenStoreHelper.AddOrUpdateAnilistToken(discordUserId, AnilistToken.Create(discordUserId, newProfileResponse.Data.ToString(), tokenResponse.access_token, tokenResponse.refresh_token));
        return BasicResult.AsSuccess();
    }

    public async Task<ApiResult<AnilistMappedDto>> GetAnilistProfileForApi(ulong discordUserId)
    {
        var userCache = await _tokenStoreHelper.GetAnilistTokens(discordUserId);

        if (userCache == null)
        {
            return ApiResult<AnilistMappedDto>.AsErrorDisplayFriendlyMessage("Anilist user registered to the provider discord user Id does not exist", HttpStatusCode.NotFound);
        }

        var profileResult = await _anilistCacheService.GetCachedAnilistProfile(discordUserId);

        if (profileResult.ResultOutcome != ResultEnum.Success)
        {
            
            return ApiResult<AnilistMappedDto>.AsInternalError();
        }

        var dto = profileResult.Data.MapToApiDto();
        
        return ApiResult<AnilistMappedDto>.AsSuccess(dto);
        
    }
    
    
}