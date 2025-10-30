using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ShoukoV2.Helpers;
using ShoukoV2.Integrations.Spotify.Interfaces;
using ShoukoV2.Models;
using ShoukoV2.Models.Configuration;
using ShoukoV2.Models.Enums;
using ShoukoV2.Models.Spotify;

namespace ShoukoV2.Integrations.Spotify;

// https://developer.spotify.com/documentation/web-api/tutorials/code-flow

public class SpotifyApiService : ISpotifyApiService
{
    private readonly ILogger<SpotifyApiService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AppMemoryStore _appMemoryStore;
    
    public SpotifyApiService(ILogger<SpotifyApiService> logger, IConfiguration configuration, IHttpClientFactory httpClientFactory,
        AppMemoryStore appMemoryStore)
    {
        _logger = logger;
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _appMemoryStore = appMemoryStore;
    }

    public async Task<Result<string>> GetAccessToken()
    {
        try
        {
            string? clientId = _configuration["SpotifyClientId"];
            string? clientSecret = _configuration["SpotifyClientSecret"];
            if (clientId == null || clientSecret == null)
            {
                _logger.LogApplicationError(DateTime.UtcNow, "Spotify client ID or secret is not configured. " +
                                                             "Please restart the application and ensure values are provided correctly");
                return Result<string>.AsError("Spotify client ID or secret is not configured. " +
                                              "Please restart the application and ensure values are provided correctly");
            }

            var request = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token");
            var formData = new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret
            };

            request.Content = new FormUrlEncodedContent(formData);
            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogApplicationError(DateTime.UtcNow, "Error getting access token from Spotify API");
                return Result<string>.AsError("Error getting access token from Spotify API");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var tokenResponse = System.Text.Json.JsonSerializer.Deserialize<SpotifyTokenResponse>(responseContent);
            if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.access_token))
            {
                _logger.LogApplicationError(DateTime.UtcNow, "Invalid token response from Spotify API");
                return Result<string>.AsError("Invalid token response from Spotify");
            }

            _logger.LogInformation("Successfully got access token from Spotify API");
            return Result<string>.AsSuccess(tokenResponse.access_token);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationException(DateTime.UtcNow, ex, "Exception when getting Spotify access token");
            return Result<string>.AsError("Exception when getting Spotify access token");
        }
    }

    // https://developer.spotify.com/documentation/web-api/reference/get-current-users-profile
    public async Task<Result<SpotifyMeResponse>> GetSpotifyProfileInfo()
    {
        try
        {
            var bearerToken = _appMemoryStore.SpotifyTokenStore.GetAccessToken();

            if (string.IsNullOrEmpty(bearerToken))
            {
                _logger.LogApplicationError(DateTime.UtcNow,
                    "No Bearer token found for Spotify API. Users needs to authenticate first");
                return Result<SpotifyMeResponse>.AsFailure(
                    "No Bearer token found for Spotify API. Users needs to authenticate first");
            }

            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.spotify.com/v1/me");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogApplicationError(DateTime.UtcNow,
                    "Unsuccessful response from Spotify API when attempting to get Spotify profile info");
                return Result<SpotifyMeResponse>.AsError(
                    "Unsuccessful response from Spotify API when attempting to get Spotify profile info");
            }

            var responseContent = await response.Content.ReadFromJsonAsync<SpotifyMeResponse>();

            if (responseContent == null)
            {
                _logger.LogApplicationError(DateTime.UtcNow, "Failed to deserialise the respons from the Spotify API");
                return Result<SpotifyMeResponse>.AsFailure("Failed to deserialise the respons from the Spotify API");
            }

            _logger.LogApplicationMessage(DateTime.UtcNow, "Successfully retrieved spotify profile info");
            return Result<SpotifyMeResponse>.AsSuccess(responseContent);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationException(DateTime.UtcNow, ex, "Exception when getting Spotify profile info");
            return Result<SpotifyMeResponse>.AsError("Exception when getting Spotify profile info");
        }
    }

    public async Task RegisterSpotifyUserId()
    {
        var userProfileDataResult = await GetSpotifyProfileInfo();

        if (userProfileDataResult.ResultOutcome == ResultEnum.Success)
        {
            _logger.LogApplicationMessage(DateTime.UtcNow, "Successfully registered Spotify user profile");
            _appMemoryStore.SpotifyTokenStore.RegisterSpotifyId(userProfileDataResult.Data.id);
        }
    }

    public async Task ValidateAndRefreshToken()
    {
        var issueDate = _appMemoryStore.SpotifyTokenStore.GetTokenIssueDate();
        var expiryDate = issueDate.AddHours(1);

        if (DateTime.UtcNow >= expiryDate)
        {
            await RefreshAccessToken();
        }
    }

    // https://developer.spotify.com/documentation/web-api/tutorials/refreshing-tokens
    private async Task RefreshAccessToken()
    {
        try
        {
            var clientId = _configuration["SpotifyClientId"];
            var clientSecret = _configuration["SpotifyClientSecret"];
            var refreshToken = _appMemoryStore.SpotifyTokenStore.GetRefreshToken();
            var base64AuthHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));

            var tokenRequest = new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = refreshToken,
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token")
            {
                Content = new FormUrlEncodedContent(tokenRequest)
            };
            request.Headers.Add("Authorization", $"Basic {base64AuthHeader}");
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.SendAsync(request);
            // rip guard programming
            // lowkey smells
            if (response.IsSuccessStatusCode)
            {
                var tokenResponse = await response.Content.ReadFromJsonAsync<SpotifyTokenResponse>();
                if (tokenResponse != null)
                {
                    // Use existing token as spotify generally returns "" for the refresh
                    var newRefreshToken = string.IsNullOrEmpty(tokenResponse.refresh_token)
                        ? refreshToken : tokenResponse.refresh_token;
                        
                    
                    _appMemoryStore.SpotifyTokenStore.RegisterAccessToken(tokenResponse.access_token, newRefreshToken, TokenType.Bearer);
                    _logger.LogApplicationMessage(DateTime.UtcNow, "Successfully refreshed Spotify access token");
                }
                else
                {
                    _logger.LogApplicationError(DateTime.UtcNow, "Failed to deserialise access token from Spotify API");
                }
            }
            else
            {
                _logger.LogApplicationError(DateTime.UtcNow,
                    "Unsuccessful response from Spotify Api when attempting to refresh token");
            }
        }
        catch (Exception ex)
        {
            _logger.LogApplicationException(DateTime.UtcNow, ex, "Exception when refreshing Spotify access token");
        }
    }

    // https://developer.spotify.com/documentation/web-api/reference/get-recently-played
    public async Task<Result<SpotifyRecentlyPlayedResponse>> GetSpotifyUserRecentlyPlayed(int limit = 10)
    {
        try
        {
            var bearerToken = _appMemoryStore.SpotifyTokenStore.GetAccessToken();

            if (string.IsNullOrEmpty(bearerToken))
            {
                _logger.LogApplicationError(DateTime.UtcNow,
                    "No Bearer token found for Spotify API. Users needs to authenticate first");
                return Result<SpotifyRecentlyPlayedResponse>.AsFailure(
                    "No Bearer token found for Spotify API. Users needs to authenticate first");
            }

            var request = new HttpRequestMessage(HttpMethod.Get,
                $"https://api.spotify.com/v1/me/player/recently-played?limit={limit}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogApplicationError(DateTime.UtcNow,
                    "Unsuccessful response from Spotify API when attempting to get Spotify recently played");
                return Result<SpotifyRecentlyPlayedResponse>.AsFailure(
                    "Unsuccessful response from Spotify API when attempting to get Spotify recently played");
            }

            var responseContent = await response.Content.ReadFromJsonAsync<SpotifyRecentlyPlayedResponse>();

            if (responseContent == null)
            {
                return Result<SpotifyRecentlyPlayedResponse>.AsFailure(
                    "Failed to deserialise Spotify response for recently played");
            }

            _logger.LogApplicationMessage(DateTime.UtcNow, "Successfully retrieved Spotify recently played");
            return Result<SpotifyRecentlyPlayedResponse>.AsSuccess(responseContent);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationException(DateTime.UtcNow, ex, "Exception when getting Spotify recently played");
            return  Result<SpotifyRecentlyPlayedResponse>.AsFailure("Exception when getting Spotify recently played");
        }
    }

    // https://developer.spotify.com/documentation/web-api/reference/get-list-users-playlists
    public async Task<Result<SpotifyUserPlaylistsResponse>> GetSpotifyUserPlaylists()
    {
        try
        {
            var bearerToken = _appMemoryStore.SpotifyTokenStore.GetAccessToken();

            if (string.IsNullOrEmpty(bearerToken))
            {
                _logger.LogApplicationError(DateTime.UtcNow,
                    "No Bearer token found for Spotify API. Users needs to authenticate first");
                return Result<SpotifyUserPlaylistsResponse>.AsFailure(
                    "No Bearer token found for Spotify API. Users needs to authenticate first");
            }

            var userId = _appMemoryStore.SpotifyTokenStore.GetUserId();

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogApplicationError(DateTime.UtcNow,
                    "No user id found for Spotify API. Users needs to authenticate first");
                return Result<SpotifyUserPlaylistsResponse>.AsFailure("No User Id Stored");
            }

            var request =
                new HttpRequestMessage(HttpMethod.Get, $"https://api.spotify.com/v1/users/{userId}/playlists");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogApplicationError(DateTime.UtcNow,
                    "Unsuccessful response from Spotify API when attempting to get Spotify User Playlist");
                return Result<SpotifyUserPlaylistsResponse>.AsFailure(
                    "Unsuccessful response from Spotify API when attempting to get Spotify User Playlist");
            }


            var responseContent = await response.Content.ReadFromJsonAsync<SpotifyUserPlaylistsResponse>();

            if (responseContent == null)
            {
                _logger.LogApplicationError(DateTime.UtcNow,
                    "Failed to deserialise Spotify response for user playlists");
                return Result<SpotifyUserPlaylistsResponse>.AsFailure(
                    "Failed to deserialise Spotify response for user playlists");
            }

            _logger.LogApplicationMessage(DateTime.UtcNow, "Successfully retrieved Spotify user playlists");
            return Result<SpotifyUserPlaylistsResponse>.AsSuccess(responseContent);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationException(DateTime.UtcNow, ex, "Exception when getting Spotify user playlists");
            return Result<SpotifyUserPlaylistsResponse>.AsFailure("Exception when getting Spotify user playlists");
        }
    }
    

}