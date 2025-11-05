using System.Text.RegularExpressions;
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
    private readonly IDiscordBusinessService _discordBusinessService;
    private readonly IAggregateBusinessService _aggregateBusinessService;
    private readonly ISteamBusinessService _steamBusinessService;

    public UserController(ILogger<UserController> logger, IAnilistBusinessService anilistBusinessService, 
        ISpotifyBusinessService spotifyBusinessService, IDiscordBusinessService discordBusinessService, 
        IAggregateBusinessService aggregateBusinessService,
        ISteamBusinessService steamBusinessService)
    {
        _logger = logger;
        _anilistBusinessService = anilistBusinessService;
        _spotifyBusinessService = spotifyBusinessService;
        _discordBusinessService = discordBusinessService;
        _aggregateBusinessService = aggregateBusinessService;
        _steamBusinessService = steamBusinessService;
    }

    [HttpGet("{userId}/spotify")]
    [AllowAnonymous]
    public async Task<IActionResult> GetUserSpotifyProfileData(ulong userId)
    {
        if (ValidateInput(userId.ToString()) == false)
        {
            return BadRequest("Missing required parameters or bad input");
        }
        
        var profileDtoResult = await _spotifyBusinessService.GetSpotifyProfileForApi(userId);

        if (profileDtoResult.ResultOutcome != ResultEnum.Success)
        {
            return StatusCode((int)profileDtoResult.StatusCode, profileDtoResult.ErrorMessage);
        }
        return Ok(profileDtoResult.Data);
    }

    [HttpGet("{userId}/anilist")]
    [AllowAnonymous]
    public async Task<IActionResult> GetUserAnilistProfileData(ulong userId)
    {
        if (ValidateInput(userId.ToString()) == false)
        {
            return BadRequest("Missing required parameters or bad input");
        }
        
        var profileDtoResult = await _anilistBusinessService.GetAnilistProfileForApi(userId);

        if (profileDtoResult.ResultOutcome != ResultEnum.Success)
        {
            return StatusCode((int)profileDtoResult.StatusCode, profileDtoResult.ErrorMessage);
        }
        return Ok(profileDtoResult.Data);
    }

    [HttpGet("{userId}/discord")]
    [AllowAnonymous]
    public async Task<IActionResult> GetUserDiscordPresence(ulong userId)
    {
        if (ValidateInput(userId.ToString()) == false)
        {
            return BadRequest("Missing required parameters or bad input");
        }
        var presenceResult = await _discordBusinessService.GetDiscordPresence(userId);

        if (presenceResult.ResultOutcome != ResultEnum.Success)
        {
            return StatusCode((int)presenceResult.StatusCode, presenceResult.ErrorMessage);
            
        }
        return Ok(presenceResult.Data);
    }

    [HttpGet("{userId}/all/")]
    [AllowAnonymous]
    public async Task<IActionResult> GetUserAll(ulong userId, [FromQuery] string? steamId = null)
    {
        if (ValidateInput(userId.ToString()) == false || ValidateNullableInput(steamId) == false)
        {
            return BadRequest("Missing required parameters or bad input");
        }
        ulong? ulongSteamId = null;
        
        if (steamId != null)
        {
            ulongSteamId = await _steamBusinessService.MapSteamIdToUniqueSteamId(steamId);
        }

        var allResult = await _aggregateBusinessService.GetAllProfileDataDto(userId, ulongSteamId);

        if (allResult.ResultOutcome != ResultEnum.Success)
        {
            return StatusCode((int)allResult.StatusCode, allResult.ErrorMessage);
        }
        return Ok(allResult.Data);
    }

    [HttpGet("{userId}/steam/{steamId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetUserSteamData(ulong userId, string steamId)
    {
        if (ValidateInput(userId.ToString()) == false || ValidateInput(steamId) == false)
        {
            return BadRequest("Missing required parameters or bad input");
        }
        
        ulong? ulongSteamId = await _steamBusinessService.MapSteamIdToUniqueSteamId(steamId);

        if (ulongSteamId == null)
        {
            return StatusCode(404, "User with the provided steamId was not found or profile is private");
        }
        
        var steamUserDataResult = await _steamBusinessService.GetSteamDataForApi(ulongSteamId.Value);

        if (steamUserDataResult.ResultOutcome != ResultEnum.Success)
        {
            return StatusCode((int)steamUserDataResult.StatusCode, steamUserDataResult.ErrorMessage);
        }
        return Ok(steamUserDataResult.Data);
    }
    
    // Validate as required
    
    private bool ValidateInput(string? input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return false;
        }
        
        input = input.Trim();

        if (input.Length > 32)
        {
            // Steam and Discord Id are 17 characters long
            // Off instance this may be longer is vanity steam url
            // keep a conservative limit
            return false;
        }

        // Only allowed a-z, A-Z, 0-9 , _, -
        if (!Regex.IsMatch(input, @"^[a-zA-Z0-9_-]+$"))
        {
            return false;
        }

        return true;
    }
    private bool ValidateNullableInput(string? input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return true;
        }
        input = input.Trim();

        if (input.Length > 32)
        {
            // Steam and Discord Id are 17 characters long
            // Off instance this may be longer is vanity steam url
            // keep a conservative limit
            return false;
        }

        // Only allowed a-z, A-Z, 0-9 , _, -
        if (!Regex.IsMatch(input, @"^[a-zA-Z0-9_-]+$"))
        {
            return false;
        }

        return true;
    }
    
    
    

    
}