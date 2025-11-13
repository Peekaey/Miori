using System.Net;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Miori.BusinessService.Interfaces;
using Miori.Cache;
using Miori.Helpers;
using Miori.Integrations.Spotify.Interfaces;
using Miori.Models;
using Miori.Models.Configuration;
using Miori.Models.Enums;
using Miori.Models.Spotify;
using Miori.TokenStore;

namespace Miori.BusinessService;

public class SpotifyBusinessService : ISpotifyBusinessService
{
    private readonly ILogger<SpotifyBusinessService> _logger;
    private readonly ISpotifyApiService  _spotifyApiService;
    private readonly HybridCache _hybridCache;
    private readonly IConfiguration _configuration;
    private readonly ISpotifyCacheService  _spotifyCacheService;
    private readonly ITokenStoreHelpers _tokenStoreHelpers;
    
    public SpotifyBusinessService(ILogger<SpotifyBusinessService> logger, ISpotifyApiService spotifyApiService, HybridCache hybridCache, 
        IConfiguration configuration,ISpotifyCacheService spotifyCacheService, ITokenStoreHelpers tokenStoreHelpers )
    { 
        _logger = logger;
        _spotifyApiService = spotifyApiService;
        _hybridCache = hybridCache;
        _configuration = configuration;
        _spotifyCacheService = spotifyCacheService;
        _tokenStoreHelpers = tokenStoreHelpers;
    }
    

    public async Task<BasicResult> RegisterNewSpotifyUser(ulong discordUserId, SpotifyTokenResponse tokenResponse)
    {
        var newProfileResponse = await  _spotifyApiService.GetSpotifyProfileIdForNewRegister(tokenResponse);

        if (newProfileResponse.ResultOutcome != ResultEnum.Success)
        {
            _logger.LogApplicationError(DateTime.UtcNow, $"Error getting spotify profile for discord user {discordUserId} with error: {newProfileResponse.ErrorMessage}");
        }
        
        _logger.LogApplicationMessage(DateTime.UtcNow, $"Successfully tied Discord User Id : '{discordUserId}' to Spotify User : '{newProfileResponse.Data}'");
        await _tokenStoreHelpers.AddOrUpdateSpotifyToken(discordUserId, SpotifyToken.Create(discordUserId, newProfileResponse.Data, tokenResponse.access_token, tokenResponse.refresh_token));
        return BasicResult.AsSuccess();
    }

    public async Task<ApiResult<SpotifyMappedDto>> GetSpotifyProfileForApi(ulong discordUserId)
    {
        var userCache = await _tokenStoreHelpers.GetSpotifyTokens(discordUserId);

        if (userCache == null)
        {
            return ApiResult<SpotifyMappedDto>.AsErrorDisplayFriendlyMessage("Spotify user registered to the provided discord user Id does not exist", HttpStatusCode.NotFound);
        }

        var profileResult = await _spotifyCacheService.GetCachedSpotifyProfile(discordUserId);

        if (profileResult.ResultOutcome != ResultEnum.Success)
        {
            return ApiResult<SpotifyMappedDto>.AsInternalError();
        }

        var dto = profileResult.Data.MapToApiDto();
        return ApiResult<SpotifyMappedDto>.AsSuccess(dto);
    }
    
    
    
}