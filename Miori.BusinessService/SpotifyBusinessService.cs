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

namespace Miori.BusinessService;

public class SpotifyBusinessService : ISpotifyBusinessService
{
    private readonly ILogger<SpotifyBusinessService> _logger;
    private readonly ISpotifyApiService  _spotifyApiService;
    private readonly HybridCache _hybridCache;
    private readonly IConfiguration _configuration;
    private readonly AppMemoryStore _appMemoryStore;
    private readonly ISpotifyCacheService  _spotifyCacheService;
    
    public SpotifyBusinessService(ILogger<SpotifyBusinessService> logger, ISpotifyApiService spotifyApiService, HybridCache hybridCache, 
        IConfiguration configuration, AppMemoryStore appMemoryStore, ISpotifyCacheService spotifyCacheService)
    { 
        _logger = logger;
        _spotifyApiService = spotifyApiService;
        _hybridCache = hybridCache;
        _configuration = configuration;
        _appMemoryStore = appMemoryStore;
        _spotifyCacheService = spotifyCacheService;
    }
    

    public async Task<BasicResult> RegisterNewSpotifyUser(ulong discordUserId, SpotifyTokenResponse tokenResponse)
    {
        var newProfileResponse = await  _spotifyApiService.GetSpotifyProfileIdForNewRegister(tokenResponse);

        if (newProfileResponse.ResultOutcome != ResultEnum.Success)
        {
            _logger.LogApplicationError(DateTime.UtcNow, $"Error getting spotify profile for discord user {discordUserId} with error: {newProfileResponse.ErrorMessage}");
        }
        
        _logger.LogApplicationMessage(DateTime.UtcNow, $"Successfully tied Discord User Id : '{discordUserId}' to Spotify User : '{newProfileResponse.Data}'");
        _appMemoryStore.AddOrUpdateSpotifyToken(discordUserId, SpotifyToken.Create(discordUserId, newProfileResponse.Data, tokenResponse.access_token, tokenResponse.refresh_token));
        return BasicResult.AsSuccess();
    }

    public async Task<ApiResult<SpotifyProfileDto>> GetSpotifyProfileForApi(ulong discordUserId)
    {
        var isSpotifyFound = _appMemoryStore.TryGetSpotifyToken(discordUserId, out var spotifyToken);

        if (isSpotifyFound == false)
        {
            return ApiResult<SpotifyProfileDto>.AsErrorDisplayFriendlyMessage("Spotify user registered to the provided discord user Id does not exist", HttpStatusCode.NotFound);
        }

        var profileResult = await _spotifyCacheService.GetCachedSpotifyProfile(discordUserId);

        if (profileResult.ResultOutcome != ResultEnum.Success)
        {
            return ApiResult<SpotifyProfileDto>.AsInternalError();
        }
        return ApiResult<SpotifyProfileDto>.AsSuccess(profileResult.Data);
    }
    
    
    
}