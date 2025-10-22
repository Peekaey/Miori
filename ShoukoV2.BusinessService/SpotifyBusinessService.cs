using Microsoft.Extensions.Logging;
using ShoukoV2.BusinessService.Interfaces;
using ShoukoV2.Integrations.Spotify.Interfaces;
using ShoukoV2.Models;
using ShoukoV2.Models.Enums;
using ShoukoV2.Models.Spotify;

namespace ShoukoV2.BusinessService;

public class SpotifyBusinessService : ISpotifyBusinessService
{
    private readonly ILogger<SpotifyBusinessService> _logger;
    private readonly ISpotifyApiService  _spotifyApiService;
    
    public SpotifyBusinessService(ILogger<SpotifyBusinessService> logger, ISpotifyApiService spotifyApiService)
    { 
        _logger = logger;
        _spotifyApiService = spotifyApiService;
    }
    
    // TODO Ideally we would want to query this on a semi regular basis and then cache this
    // as to not get rate limited and improve response time
    // this data does not refresh that often, apart from recently played
    public async Task<Result<SpotifyProfileDto>> GetSpotifyProfile()
    {
        try
        {
            await _spotifyApiService.ValidateAndRefreshToken();
            SpotifyProfileDto spotifyProfileDto = new SpotifyProfileDto();
            var spotifyProfileResult = await _spotifyApiService.GetSpotifyProfileInfo();
            var spotifyRecentlyPlayedResult = await _spotifyApiService.GetSpotifyUserRecentlyPlayed();
            var spotifyPlaylistsResult = await _spotifyApiService.GetSpotifyUserPlaylists();

            if (spotifyProfileResult.ResultOutcome == ResultEnum.Success)
            {
                spotifyProfileDto.SpotifyProfile = spotifyProfileResult.Data;
            }

            if (spotifyRecentlyPlayedResult.ResultOutcome == ResultEnum.Success)
            {
                spotifyProfileDto.RecentlyPlayed = spotifyRecentlyPlayedResult.Data;
            }

            if (spotifyPlaylistsResult.ResultOutcome == ResultEnum.Success)
            {
                spotifyProfileDto.UserPlaylists = spotifyPlaylistsResult.Data;
            }
            return Result<SpotifyProfileDto>.AsSuccess(spotifyProfileDto);

        } catch (Exception ex)
        {
            return Result<SpotifyProfileDto>.AsError(ex.Message);
        }
    }
    
    
}