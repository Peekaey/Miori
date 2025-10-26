using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ShoukoV2.BusinessService.Interfaces;
using ShoukoV2.Models.Enums;

namespace ShoukoV2.Api.Rest;

[ApiController]
[RequestLoggingFilter]
[ApiKeyAuth]
[Route("api/v1/[Controller]")]
public class ProfileController : ControllerBase
{
    private readonly ILogger<ProfileController> _logger;
    private readonly IAnilistBusinessService _anilistBusinessService;
    private readonly ISpotifyBusinessService _spotifyBusinessService;
    private readonly IUnraidBusinessService _unraidBusinessService;
    private readonly IDiscordBusinessService _discordBusinessService;
    private readonly IAggregateBusinessService _aggregateBusinessService;

    public ProfileController(ILogger<ProfileController> logger, IAnilistBusinessService anilistBusinessService, 
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

    [HttpGet("spotify")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSpotifyProfileData()
    {
        var profileDtoResult = await _spotifyBusinessService.GetCachedSpotifyProfile();

        if (profileDtoResult.ResultOutcome != ResultEnum.Success)
        {
            return StatusCode(500, "Internal error occured when attempting to get spotify data");
        }
        return Ok(profileDtoResult.Data);
    }

    [HttpGet("anilist")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAnilistProfileData()
    {
        var profileDtoResult = await _anilistBusinessService.GetCachedAnilistProfile();

        if (profileDtoResult.ResultOutcome != ResultEnum.Success)
        {
            return StatusCode(500, "Internal error occured when attempting to get anilist data");
        }
        return Ok(profileDtoResult.Data);
    }

    [HttpGet("discord")]
    [AllowAnonymous]
    public async Task<IActionResult> GetDiscordPresence()
    {
        var presenceResult = await _discordBusinessService.GetDiscordPresence();

        if (presenceResult.ResultOutcome != ResultEnum.Success)
        {
            return StatusCode(500, "Internal error occured when attempting to get discord presence data");
        }
        return Ok(presenceResult.Data);
    }

    [HttpGet("all")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var allResult = await _aggregateBusinessService.GetAllProfileDataDto();

        if (allResult.ResultOutcome != ResultEnum.Success)
        {
            return StatusCode(500, "Internal error occured when attempting to get all profile data");
        }
        return Ok(allResult.Data);
    }
    
}