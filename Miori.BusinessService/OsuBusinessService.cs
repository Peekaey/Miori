using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Miori.BusinessService.Interfaces;
using Miori.Cache.Interfaces;
using Miori.Helpers;
using Miori.Integrations.Osu;
using Miori.Models;
using Miori.Models.Configuration;
using Miori.Models.Enums;
using Miori.Models.Osu;
using Miori.TokenStore;

namespace Miori.BusinessService;

public class OsuBusinessService : IOsuBusinessService
{
    private readonly ILogger<OsuBusinessService> _logger;
    private readonly IConfiguration _configuration;
    private readonly ITokenStoreHelpers _tokenStoreHelper;
    private readonly IOsuCacheService _osuCacheService;
    private readonly IOsuApiService  _osuApiService;
    

    public OsuBusinessService(ILogger<OsuBusinessService> logger, IConfiguration configuration,
        ITokenStoreHelpers tokenStoreHelper, IOsuCacheService osuCacheService,
        IOsuApiService osuApiService)
    {
        _logger = logger;
        _configuration = configuration;
        _tokenStoreHelper = tokenStoreHelper;
        _osuCacheService = osuCacheService;
        _osuApiService = osuApiService;
    }

    public async Task<BasicResult> RegisterNewOsuUser(ulong discordUserId, OsuTokenResponse tokenResponse)
    {
        var newProfileResponse = await _osuApiService.GetOsuProfileIdForNewRegister(tokenResponse);

        if (newProfileResponse.ResultOutcome != ResultEnum.Success)
        {
            _logger.LogApplicationError(DateTime.UtcNow, $"Error getting osu profile for {discordUserId} with error: {newProfileResponse.ErrorMessage}");
        }
        
        _logger.LogApplicationMessage(DateTime.UtcNow, $"Successfully tied DiscordUser Id: '{discordUserId} to Osu User : '{newProfileResponse.Data}");
        await _tokenStoreHelper.AddOrUpdateOsuToken(discordUserId, OsuToken.Create(discordUserId, newProfileResponse.Data.ToString(), tokenResponse.access_token, tokenResponse.refresh_token));
        return BasicResult.AsSuccess();
    }

    public async Task<ApiResult<OsuMappedDto>> GetOsuProfileForApi(ulong discordUserId)
    {
        var userCache = await _tokenStoreHelper.GetOsuTokens(discordUserId);

        if (userCache == null)
        {
            return ApiResult<OsuMappedDto>.AsErrorDisplayFriendlyMessage("Osu user registered to the provider discord user Id does not exist", HttpStatusCode.NotFound);
        }

        var profileResult = await _osuCacheService.GetCachedOsuProfile(discordUserId);

        if (profileResult.ResultOutcome != ResultEnum.Success)
        {
            return ApiResult<OsuMappedDto>.AsInternalError();
        }
        
        var dto = profileResult.Data.MapToApiDto();
        return ApiResult<OsuMappedDto>.AsSuccess(dto);
    }
}