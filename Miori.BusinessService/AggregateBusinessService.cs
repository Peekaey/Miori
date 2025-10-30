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

    public AggregateBusinessService(ILogger<AggregateBusinessService> logger,
        IDiscordBusinessService discordBusinessService,
        ISpotifyBusinessService spotifyBusinessService, IAnilistBusinessService anilistBusinessService,
        IUnraidBusinessService unraidBusinessService)
    {
        _logger = logger;
        _discordBusinessService = discordBusinessService;
        _spotifyBusinessService = spotifyBusinessService;
        _anilistBusinessService = anilistBusinessService;
        _unraidBusinessService = unraidBusinessService;
    }
        

    public async Task<Result<AggregateProfileDto>> GetAllProfileDataDto()
    {
        try
        {
            var discordProfileDtoTask = _discordBusinessService.GetDiscordPresence();
            var anilistProfileDtoTask = _anilistBusinessService.GetCachedAnilistProfile();
            var spotifyProfileDtoTask = _spotifyBusinessService.GetCachedSpotifyProfile();

            await Task.WhenAll(discordProfileDtoTask, anilistProfileDtoTask, spotifyProfileDtoTask);

            var discordProfileDtoResult = await discordProfileDtoTask;
            var anilistProfileDtoResult = await anilistProfileDtoTask;
            var spotifyProfileDtoResult = await spotifyProfileDtoTask;

            var aggregateprofileData = new AggregateProfileDto();
            
            // TODO when returning aggregate data, we want to reduce the noise that is returned.
            // Therefore if the user is not authenticated or no data is returned, do not return the empty dto back to the
            // controller api response
            
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

            return Result<AggregateProfileDto>.AsSuccess(aggregateprofileData);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationException(DateTime.UtcNow, ex, "Exception when getting aggregated profile data");
            return Result<AggregateProfileDto>.AsError("Exception when getting aggregated profile data");
        }
    }
    
}