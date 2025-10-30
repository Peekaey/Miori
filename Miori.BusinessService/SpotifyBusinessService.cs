using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Miori.BusinessService.Interfaces;
using Miori.Helpers;
using Miori.Integrations.Spotify.Interfaces;
using Miori.Models;
using Miori.Models.Enums;
using Miori.Models.Spotify;

namespace Miori.BusinessService;

public class SpotifyBusinessService : ISpotifyBusinessService
{
    private readonly ILogger<SpotifyBusinessService> _logger;
    private readonly ISpotifyApiService  _spotifyApiService;
    private readonly HybridCache _hybridCache;
    private readonly IConfiguration _configuration;
    
    public SpotifyBusinessService(ILogger<SpotifyBusinessService> logger, ISpotifyApiService spotifyApiService, HybridCache hybridCache, 
        IConfiguration configuration)
    { 
        _logger = logger;
        _spotifyApiService = spotifyApiService;
        _hybridCache = hybridCache;
        _configuration = configuration;
    }
    
    public async Task<Result<SpotifyProfileDto>> GetCachedSpotifyProfile()
    {
        try
        {
            var enableCaching = _configuration.GetValue<bool>("EnableCaching");

            if (enableCaching == true)
            {
                var cachedData = await _hybridCache.GetOrCreateAsync(
                    CacheKeys.SpotifyProfile,
                    async cancellationToken =>
                    {
                        _logger.LogInformation("Cache miss - fetching latest Spotify profile data");
                        return await FetchSpotifyProfileFromApiConcurrently();
                    },
                    new HybridCacheEntryOptions
                    {
                        Expiration = TimeSpan.FromMinutes(30),
                        LocalCacheExpiration = TimeSpan.FromMinutes(30)
                    });

                return Result<SpotifyProfileDto>.AsSuccess(cachedData);
            }
            else
            {
                var profileData = await FetchSpotifyProfileFromApiConcurrently();
                return Result<SpotifyProfileDto>.AsSuccess(profileData);
            }
            
        }
        catch (Exception ex)
        {
            _logger.LogApplicationException(DateTime.UtcNow, ex, "Error fetching Spotify profile data");
            return Result<SpotifyProfileDto>.AsError(ex.Message);
        }
    }
    
    public async Task<SpotifyProfileDto> FetchSpotifyProfileFromApiConcurrently()
    {
        await _spotifyApiService.ValidateAndRefreshToken();
        
        var spotifyProfileDto = new SpotifyProfileDto();

        var spotifyProfileTask = _spotifyApiService.GetSpotifyProfileInfo();
        var spotifyRecentlyPlayedTask =  _spotifyApiService.GetSpotifyUserRecentlyPlayed();
        var spotifyPlaylistsTask = _spotifyApiService.GetSpotifyUserPlaylists();
        
        await Task.WhenAll(spotifyProfileTask, spotifyRecentlyPlayedTask, spotifyPlaylistsTask);

        var spotifyProfileResult = await spotifyProfileTask;
        var spotifyRecentlyPlayedResult = await spotifyRecentlyPlayedTask;
        var spotifyPlaylistsResult = await spotifyPlaylistsTask;
        
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
        
        return spotifyProfileDto;
    }
    

    // Legacy Test Call for Debugging
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
            _logger.LogApplicationException(DateTime.UtcNow, ex, "Error fetching Spotify profile data");
            return Result<SpotifyProfileDto>.AsError(ex.Message);
        }
    }
    
    
    
}