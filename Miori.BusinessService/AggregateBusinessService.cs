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
    private readonly ISteamBusinessService _steamBusinessService;
    private readonly IOsuBusinessService _osuBusinessService;

    public AggregateBusinessService(ILogger<AggregateBusinessService> logger,
        IDiscordBusinessService discordBusinessService,
        ISpotifyBusinessService spotifyBusinessService, IAnilistBusinessService anilistBusinessService,
        ISteamBusinessService steamBusinessService, IOsuBusinessService osuBusinessService)
    {
        _logger = logger;
        _discordBusinessService = discordBusinessService;
        _spotifyBusinessService = spotifyBusinessService;
        _anilistBusinessService = anilistBusinessService;
        _steamBusinessService = steamBusinessService;
        _osuBusinessService = osuBusinessService;
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
                var osuProfileDtoTask = _osuBusinessService.GetOsuProfileForApi(discordUserId);
                
                await Task.WhenAll(discordProfileDtoTask, anilistProfileDtoTask, spotifyProfileDtoTask, steamDataDtoTask,osuProfileDtoTask);

                var discordProfileDtoResult = await discordProfileDtoTask;
                var anilistProfileDtoResult = await anilistProfileDtoTask;
                var spotifyProfileDtoResult = await spotifyProfileDtoTask;
                var steamDataDtoResult = await steamDataDtoTask;
                var osuProfileDtoResult = await osuProfileDtoTask;
                
                if (discordProfileDtoResult.ResultOutcome == ResultEnum.Success)
                {
                    aggregateprofileData.DiscordUserData = discordProfileDtoResult.Data;
                }

                if (anilistProfileDtoResult.ResultOutcome == ResultEnum.Success)
                {
                    aggregateprofileData.AnilistUserData = anilistProfileDtoResult.Data;
                }

                if (spotifyProfileDtoResult.ResultOutcome == ResultEnum.Success)
                {
                    aggregateprofileData.SpotifyUserData = spotifyProfileDtoResult.Data;
                }

                if (steamDataDtoResult.ResultOutcome == ResultEnum.Success)
                {
                    aggregateprofileData.SteamUserData = steamDataDtoResult.Data;
                }

                if (osuProfileDtoResult.ResultOutcome == ResultEnum.Success)
                {
                    aggregateprofileData.OsuUserData = osuProfileDtoResult.Data;
                }
            }
            else
            {
                var discordProfileDtoTask = _discordBusinessService.GetDiscordPresence(discordUserId);
                var anilistProfileDtoTask = _anilistBusinessService.GetAnilistProfileForApi(discordUserId);
                var spotifyProfileDtoTask = _spotifyBusinessService.GetSpotifyProfileForApi(discordUserId);
                var osuProfileDtoTask = _osuBusinessService.GetOsuProfileForApi(discordUserId);
                
                await Task.WhenAll(discordProfileDtoTask, anilistProfileDtoTask, spotifyProfileDtoTask, osuProfileDtoTask);

                var discordProfileDtoResult = await discordProfileDtoTask;
                var anilistProfileDtoResult = await anilistProfileDtoTask;
                var spotifyProfileDtoResult = await spotifyProfileDtoTask;
                var osuProfileDtoResult = await osuProfileDtoTask;

            
                if (discordProfileDtoResult.ResultOutcome == ResultEnum.Success)
                {
                    aggregateprofileData.DiscordUserData = discordProfileDtoResult.Data;
                }

                if (anilistProfileDtoResult.ResultOutcome == ResultEnum.Success)
                {
                    aggregateprofileData.AnilistUserData = anilistProfileDtoResult.Data;
                }

                if (spotifyProfileDtoResult.ResultOutcome == ResultEnum.Success)
                {
                    aggregateprofileData.SpotifyUserData = spotifyProfileDtoResult.Data;
                }

                if (osuProfileDtoResult.ResultOutcome == ResultEnum.Success)
                {
                    aggregateprofileData.OsuUserData = osuProfileDtoResult.Data;
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