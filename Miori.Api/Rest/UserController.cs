using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Miori.BusinessService.Interfaces;
using Miori.Models.Enums;

namespace Miori.Api.Rest;

[ApiController]
[RequestLoggingFilter]
[ApiKeyAuth]
[Route("api/v1/[Controller]")]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly IAnilistBusinessService _anilistBusinessService;
    private readonly ISpotifyBusinessService _spotifyBusinessService;
    private readonly IUnraidBusinessService _unraidBusinessService;
    private readonly IDiscordBusinessService _discordBusinessService;
    private readonly IAggregateBusinessService _aggregateBusinessService;

    public UserController(ILogger<UserController> logger, IAnilistBusinessService anilistBusinessService, 
        ISpotifyBusinessService spotifyBusinessService, IUnraidBusinessService unraidBusinessService, 
        IDiscordBusinessService discordBusinessService, IAggregateBusinessService aggregateBusinessService)
    {
        _logger = logger;
        _anilistBusinessService = anilistBusinessService;
        _spotifyBusinessService = spotifyBusinessService;
        _unraidBusinessService = unraidBusinessService;
        _discordBusinessService = discordBusinessService;
        _aggregateBusinessService = aggregateBusinessService;
    }

    [HttpGet("{userId}/spotify")]
    [AllowAnonymous]
    public async Task<IActionResult> GetUserSpotifyProfileData(ulong userId)
    {
        var profileDtoResult = await _spotifyBusinessService.GetSpotifyProfileForApi(userId);

        if (profileDtoResult.ResultOutcome != ResultEnum.Success)
        {
            return StatusCode(500, "Internal error occured when attempting to get spotify data");
        }
        return Ok(profileDtoResult.Data);
    }

    [HttpGet("{userId}/anilist")]
    [AllowAnonymous]
    public async Task<IActionResult> GetUserAnilistProfileData(ulong userId)
    {
        var profileDtoResult = await _anilistBusinessService.GetAnilistProfileForApi(userId);

        if (profileDtoResult.ResultOutcome != ResultEnum.Success)
        {
            return StatusCode(500, "Internal error occured when attempting to get anilist data");
        }
        return Ok(profileDtoResult.Data);
    }

    [HttpGet("{userId}/discord")]
    [AllowAnonymous]
    public async Task<IActionResult> GetUserDiscordPresence(ulong userId)
    {
        var presenceResult = await _discordBusinessService.GetDiscordPresence(userId);

        if (presenceResult.ResultOutcome != ResultEnum.Success)
        {
            return StatusCode(500, "Internal error occured when attempting to get discord presence data");
        }
        return Ok(presenceResult.Data);
    }

    [HttpGet("{userId}/all")]
    [AllowAnonymous]
    public async Task<IActionResult> GetUserAll(ulong discordUserId)
    {
        var allResult = await _aggregateBusinessService.GetAllProfileDataDto(discordUserId);

        if (allResult.ResultOutcome != ResultEnum.Success)
        {
            return StatusCode(500, "Internal error occured when attempting to get all profile data");
        }
        return Ok(allResult.Data);
    }
    
}