using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Miori.Helpers;
using Miori.Integrations.Spotify.Interfaces;
using Miori.Models;
using Miori.Models.Configuration;
using Miori.Models.Enums;
using Miori.Models.Spotify;
using Miori.TokenStore;

namespace Miori.Integrations.Spotify;

// https://developer.spotify.com/documentation/web-api/tutorials/code-flow

public class SpotifyApiService : ISpotifyApiService
{
    private readonly ILogger<SpotifyApiService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ITokenStoreHelpers _tokenStoreHelpers;
    
    public SpotifyApiService(ILogger<SpotifyApiService> logger, IConfiguration configuration, IHttpClientFactory httpClientFactory, 
        ITokenStoreHelpers tokenStoreHelpers)
    {
        _logger = logger;
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _tokenStoreHelpers = tokenStoreHelpers;
    }

    public async Task<Result<string>> GetSpotifyProfileIdForNewRegister(SpotifyTokenResponse tokenResponse)
    {
        try
        {
            
            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.spotify.com/v1/me");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenResponse.access_token);

            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogApplicationError(DateTime.UtcNow, "Unsuccessful response from Spotify API when attempting to get Spotify profile info");
                return Result<string>.AsError("Unsuccessful response from Spotify API when attempting to get Spotify profile info");
            }

            var responseContent = await response.Content.ReadFromJsonAsync<SpotifyMeResponse>();

            if (responseContent == null)
            {
                _logger.LogApplicationError(DateTime.UtcNow, "Failed to deserialise the response from the Spotify API");
                return Result<string>.AsFailure("Failed to deserialise the response from the Spotify API");
            }

            return Result<string>.AsSuccess(responseContent.id);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationException(DateTime.UtcNow, ex, "Exception when getting Spotify profile info");
            return Result<string>.AsError("Exception when getting Spotify profile info");
        }
    }

    public async Task<SpotifyProfileDto> GetSpotifyUserData(ulong discordUserId)
    {
        // Don't handle any errors, let it bubble up and handle
        await ValidateAndRefreshToken(discordUserId);
        return await FetchSpotifyDataFromApiConcurrently(discordUserId);
    }

    private async Task<SpotifyProfileDto> FetchSpotifyDataFromApiConcurrently(ulong discordUserId)
    {
        var spotifyProfileDto = new SpotifyProfileDto();

        var spotifyProfileTask = GetSpotifyProfileInfo(discordUserId);
        var spotifyRecentlyPlayedTask =  GetSpotifyUserRecentlyPlayed(discordUserId);
        var spotifyPlaylistsTask = GetSpotifyUserPlaylists(discordUserId);
        
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
    

        // https://developer.spotify.com/documentation/web-api/reference/get-current-users-profile
        private async Task<Result<SpotifyMeResponse>> GetSpotifyProfileInfo(ulong discordUserId)
    {
        try
        {
            var existingSpotifyCache = await _tokenStoreHelpers.GetSpotifyTokens(discordUserId);
            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.spotify.com/v1/me");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", existingSpotifyCache.AccessToken);

            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogApplicationError(DateTime.UtcNow, "Unsuccessful response from Spotify API when attempting to get Spotify profile info");
                return Result<SpotifyMeResponse>.AsError("Unsuccessful response from Spotify API when attempting to get Spotify profile info");
            }

            var responseContent = await response.Content.ReadFromJsonAsync<SpotifyMeResponse>();

            if (responseContent == null)
            {
                _logger.LogApplicationError(DateTime.UtcNow, "Failed to deserialise the response from the Spotify API for spotify profile info");
                return Result<SpotifyMeResponse>.AsFailure("Failed to deserialise the response from the Spotify API for spotify profile info");
            }
            
            return Result<SpotifyMeResponse>.AsSuccess(responseContent);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationException(DateTime.UtcNow, ex, "Exception when getting Spotify profile info");
            return Result<SpotifyMeResponse>.AsError("Exception when getting Spotify profile info");
        }
    }

    private async Task ValidateAndRefreshToken(ulong discordUserId)
    {
        var userSpotifyToken = await  _tokenStoreHelpers.GetSpotifyTokens(discordUserId);
        
        if (userSpotifyToken.IsExpired == true)
        {
            await RefreshAccessToken(discordUserId);
        }
    }

    // https://developer.spotify.com/documentation/web-api/tutorials/refreshing-tokens
    private async Task RefreshAccessToken(ulong discordUserId)
    {
        try
        {
            var existingSpotifyCache = await _tokenStoreHelpers.GetSpotifyTokens(discordUserId);
            
            var clientId = _configuration["SpotifyClientId"];
            var clientSecret = _configuration["SpotifyClientSecret"];
            var base64AuthHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));

            var tokenRequest = new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = existingSpotifyCache.RefreshToken,
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token")
            {
                Content = new FormUrlEncodedContent(tokenRequest)
            };
            request.Headers.Add("Authorization", $"Basic {base64AuthHeader}");
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.SendAsync(request);

            
            if (response.IsSuccessStatusCode)
            {
                var tokenResponse = await response.Content.ReadFromJsonAsync<SpotifyTokenResponse>();
                if (tokenResponse != null)
                {
                    // Use existing token as spotify generally returns "" for the refresh
                    var newRefreshToken = string.IsNullOrEmpty(tokenResponse.refresh_token) ? existingSpotifyCache.RefreshToken : tokenResponse.refresh_token;
                    var replacedTokenObject = existingSpotifyCache.WithRefreshedToken(tokenResponse.access_token, newRefreshToken);    
                    await _tokenStoreHelpers.AddOrUpdateSpotifyToken(discordUserId, replacedTokenObject);
                    
                    _logger.LogApplicationMessage(DateTime.UtcNow, $"Successfully refreshed Spotify access token for {discordUserId} with Spotify user Id : '{replacedTokenObject.SpotifyUserId}'");
                }
                else
                {
                    _logger.LogApplicationError(DateTime.UtcNow, "Failed to deserialise access token from Spotify API");
                }
            }
            else
            {
                _logger.LogApplicationError(DateTime.UtcNow, "Unsuccessful response from Spotify Api when attempting to refresh token");
            }
        }
        catch (Exception ex)
        {
            _logger.LogApplicationException(DateTime.UtcNow, ex, "Exception when refreshing Spotify access token");
        }
    }

    // https://developer.spotify.com/documentation/web-api/reference/get-recently-played
    private async Task<Result<SpotifyRecentlyPlayedResponse>> GetSpotifyUserRecentlyPlayed(ulong discordUserId, int limit = 18)
    {
        try
        {
            var existingSpotifyCache = await _tokenStoreHelpers.GetSpotifyTokens(discordUserId);
            var request = new HttpRequestMessage(HttpMethod.Get,
                $"https://api.spotify.com/v1/me/player/recently-played?limit={limit}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", existingSpotifyCache.AccessToken);

            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogApplicationError(DateTime.UtcNow, "Unsuccessful response from Spotify API when attempting to get Spotify recently played");
                return Result<SpotifyRecentlyPlayedResponse>.AsFailure("Unsuccessful response from Spotify API when attempting to get Spotify recently played");
            }

            var responseContent = await response.Content.ReadFromJsonAsync<SpotifyRecentlyPlayedResponse>();

            if (responseContent == null)
            {
                return Result<SpotifyRecentlyPlayedResponse>.AsFailure("Failed to deserialise Spotify response for recently played");
            }
            
            return Result<SpotifyRecentlyPlayedResponse>.AsSuccess(responseContent);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationException(DateTime.UtcNow, ex, "Exception when getting Spotify recently played");
            return  Result<SpotifyRecentlyPlayedResponse>.AsFailure("Exception when getting Spotify recently played");
        }
    }

    // https://developer.spotify.com/documentation/web-api/reference/get-list-users-playlists
    private async Task<Result<SpotifyUserPlaylistsResponse>> GetSpotifyUserPlaylists(ulong discordUserId)
    {
        try
        {
            var existingSpotifyCache = await _tokenStoreHelpers.GetSpotifyTokens(discordUserId);

            var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.spotify.com/v1/users/{existingSpotifyCache.SpotifyUserId}/playlists");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", existingSpotifyCache.AccessToken);

            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogApplicationError(DateTime.UtcNow, "Unsuccessful response from Spotify API when attempting to get Spotify User Playlist");
                return Result<SpotifyUserPlaylistsResponse>.AsFailure("Unsuccessful response from Spotify API when attempting to get Spotify User Playlist");
            }
            
            var responseContent = await response.Content.ReadFromJsonAsync<SpotifyUserPlaylistsResponse>();

            if (responseContent == null)
            {
                _logger.LogApplicationError(DateTime.UtcNow, "Failed to deserialise Spotify response for user playlists");
                return Result<SpotifyUserPlaylistsResponse>.AsFailure("Failed to deserialise Spotify response for user playlists");
            }

            return Result<SpotifyUserPlaylistsResponse>.AsSuccess(responseContent);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationException(DateTime.UtcNow, ex, "Exception when getting Spotify user playlists");
            return Result<SpotifyUserPlaylistsResponse>.AsFailure("Exception when getting Spotify user playlists");
        }
    }
    
}