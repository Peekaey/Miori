using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ShoukoV2.BusinessService.Interfaces;
using ShoukoV2.Models.Enums;

namespace ShoukoV2.Api;

[ApiController]
[Route("api/v1/[Controller]")]
public class ProfileController : ControllerBase
{
    private readonly ILogger<ProfileController> _logger;
    private readonly IAnilistBusinessService _anilistBusinessService;
    private readonly ISpotifyBusinessService _spotifyBusinessService;

    public ProfileController(ILogger<ProfileController> logger, IAnilistBusinessService anilistBusinessService, ISpotifyBusinessService spotifyBusinessService)
    {
        _logger = logger;
        _anilistBusinessService = anilistBusinessService;
        _spotifyBusinessService = spotifyBusinessService;
    }

    [HttpGet("spotify")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSpotifyProfileData()
    {
        var profileDtoResult = await _spotifyBusinessService.GetSpotifyProfile();

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
        var profileDtoResult = await _anilistBusinessService.GetAnilistProfile();

        if (profileDtoResult.ResultOutcome != ResultEnum.Success)
        {
            return StatusCode(500, "Internal error occured when attempting to get anilist data");
        }
        return Ok(profileDtoResult.Data);
    }
    
    
    
    
    
}