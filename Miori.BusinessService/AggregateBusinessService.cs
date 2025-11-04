using Microsoft.Extensions.Logging;
using Miori.BusinessService.Interfaces;
using Miori.Helpers;
using Miori.Models;
using Miori.Models.Enums;

namespace Miori.BusinessService;

public class AggregateBusinessService : IAggregateBusinessService
{
    private readonly ILogger<AggregateBusinessService> _logger;
    private readonly IDiscordBusinessService _discordBusinessService;
    private readonly ISpotifyBusinessService _spotifyBusinessService;
    private readonly IAnilistBusinessService _anilistBusinessService;
    private readonly IUnraidBusinessService _unraidBusinessService;
    private readonly ISteamBusinessService _steamBusinessService;

    public AggregateBusinessService(ILogger<AggregateBusinessService> logger,
        IDiscordBusinessService discordBusinessService,
        ISpotifyBusinessService spotifyBusinessService, IAnilistBusinessService anilistBusinessService,
        IUnraidBusinessService unraidBusinessService, ISteamBusinessService steamBusinessService)
    {
        _logger = logger;
        _discordBusinessService = discordBusinessService;
        _spotifyBusinessService = spotifyBusinessService;
        _anilistBusinessService = anilistBusinessService;
        _unraidBusinessService = unraidBusinessService;
        _steamBusinessService = steamBusinessService;
    }
        

    public async Task<ApiResult<AggregateProfileDto>> GetAllProfileDataDto(ulong discordUserId, ulong? steamId)
    {
        try
        {
            var aggregateprofileData = new AggregateProfileDto();

            if (steamId.HasValue)
            {
                var discordProfileDtoTask = _discordBusinessService.GetDiscordPresence(discordUserId);
                var anilistProfileDtoTask = _anilistBusinessService.GetAnilistProfileForApi(discordUserId);
                var spotifyProfileDtoTask = _spotifyBusinessService.GetSpotifyProfileForApi(discordUserId);
                var steamDataDtoTask =  _steamBusinessService.GetSteamDataForApi(steamId.Value);
            
                await Task.WhenAll(discordProfileDtoTask, anilistProfileDtoTask, spotifyProfileDtoTask, steamDataDtoTask);

                var discordProfileDtoResult = await discordProfileDtoTask;
                var anilistProfileDtoResult = await anilistProfileDtoTask;
                var spotifyProfileDtoResult = await spotifyProfileDtoTask;
                var steamDataDtoResult = await steamDataDtoTask;
                
                if (discordProfileDtoResult.ResultOutcome == ResultEnum.Success)
                {
                    aggregateprofileData.DiscordProfileData = discordProfileDtoResult.Data;
                }

                if (anilistProfileDtoResult.ResultOutcome == ResultEnum.Success)
                {
                    aggregateprofileData.AnilistProfileData = anilistProfileDtoResult.Data;
                }

                if (spotifyProfileDtoResult.ResultOutcome == ResultEnum.Success)
                {
                    aggregateprofileData.SpotifyProfileData = spotifyProfileDtoResult.Data;
                }

                if (steamDataDtoResult.ResultOutcome == ResultEnum.Success)
                {
                    aggregateprofileData.SteamApiData = steamDataDtoResult.Data;
                }
            }
            else
            {
                var discordProfileDtoTask = _discordBusinessService.GetDiscordPresence(discordUserId);
                var anilistProfileDtoTask = _anilistBusinessService.GetAnilistProfileForApi(discordUserId);
                var spotifyProfileDtoTask = _spotifyBusinessService.GetSpotifyProfileForApi(discordUserId);
            
                await Task.WhenAll(discordProfileDtoTask, anilistProfileDtoTask, spotifyProfileDtoTask);

                var discordProfileDtoResult = await discordProfileDtoTask;
                var anilistProfileDtoResult = await anilistProfileDtoTask;
                var spotifyProfileDtoResult = await spotifyProfileDtoTask;

            
                if (discordProfileDtoResult.ResultOutcome == ResultEnum.Success)
                {
                    aggregateprofileData.DiscordProfileData = discordProfileDtoResult.Data;
                }

                if (anilistProfileDtoResult.ResultOutcome == ResultEnum.Success)
                {
                    aggregateprofileData.AnilistProfileData = anilistProfileDtoResult.Data;
                }

                if (spotifyProfileDtoResult.ResultOutcome == ResultEnum.Success)
                {
                    aggregateprofileData.SpotifyProfileData = spotifyProfileDtoResult.Data;
                }
            }
            
            return ApiResult<AggregateProfileDto>.AsSuccess(aggregateprofileData);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationException(DateTime.UtcNow, ex, "Exception when getting aggregated profile data");
            return ApiResult<AggregateProfileDto>.AsInternalError();
        }
    }
    
}