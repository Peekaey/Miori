using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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

    public async Task<ApiResult<string>> GetAccessToken()
    {
        string? clientId = _configuration["Spotify:ClientId"];
        string? clientSecret = _configuration["Spotify:ClientSecret"];
        if (clientId == null || clientSecret == null)
        {
            return ApiResult<string>.AsError("Spotify client ID or secret is not configured",
                System.Net.HttpStatusCode.InternalServerError);
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
            return ApiResult<string>.AsError("Error when attempting to obtain token", response.StatusCode);
        }
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var tokenResponse = System.Text.Json.JsonSerializer.Deserialize<SpotifyTokenResponse>(responseContent);
        if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.access_token))
        {
            return ApiResult<string>.AsError("Invalid token response from Spotify", System.Net.HttpStatusCode.InternalServerError);
        }
        return ApiResult<string>.AsSuccess(tokenResponse.access_token);
    }

    // https://developer.spotify.com/documentation/web-api/reference/get-current-users-profile
    public async Task<ApiResult<SpotifyMeResponse>> GetSpotifyProfileInfo()
    {
        var bearerToken = _appMemoryStore.SpotifyTokenStore.GetAccessToken();

        if (string.IsNullOrEmpty(bearerToken))
        {
            return ApiResult<SpotifyMeResponse>.AsFailure("No Access Token Found");
        }
        
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.spotify.com/v1/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);


        
        var httpClient = _httpClientFactory.CreateClient();
        var response = await httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            return ApiResult<SpotifyMeResponse>.AsError("Non success returned from Spotify Api", response.StatusCode);
        }
        
        var responseContent = await response.Content.ReadFromJsonAsync<SpotifyMeResponse>();

        if (responseContent == null)
        {
            return ApiResult<SpotifyMeResponse>.AsFailure("Failed to deserialise the response");
        }
        
        return ApiResult<SpotifyMeResponse>.AsSuccess(responseContent);
    }

    public async Task RegisterSpotifyUserId()
    {
        var userProfileDataResult = await GetSpotifyProfileInfo();

        if (userProfileDataResult.Result == ResultEnum.Success)
        {
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
        var clientId = _configuration["Spotify:ClientId"];
        var clientSecret = _configuration["Spotify:ClientSecret"];
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
                _appMemoryStore.SpotifyTokenStore.RegisterAccessToken(tokenResponse.access_token, tokenResponse.refresh_token, TokenType.Bearer);
            }
            else
            {
                _logger.LogError("Failed to parse token response");
            }
        }
        else
        {
            _logger.LogError("Failed to refresh access token");
        }
    }

    // https://developer.spotify.com/documentation/web-api/reference/get-recently-played
    public async Task<ApiResult<SpotifyRecentlyPlayedResponse>> GetSpotifyUserRecentlyPlayed(int limit = 10)
    {
        var bearerToken = _appMemoryStore.SpotifyTokenStore.GetAccessToken();

        if (string.IsNullOrEmpty(bearerToken))
        {
            return ApiResult<SpotifyRecentlyPlayedResponse>.AsFailure("No Access Token Found");
        }
        
        var request = new HttpRequestMessage(HttpMethod.Get,
            $"https://api.spotify.com/v1/me/player/recently-played?limit={limit}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        var httpClient = _httpClientFactory.CreateClient();
        var response = await httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            return ApiResult<SpotifyRecentlyPlayedResponse>.AsFailure("Non success returned from spotify api");
        }
        
        var responseContent = await response.Content.ReadFromJsonAsync<SpotifyRecentlyPlayedResponse>();

        if (responseContent == null)
        {
            return ApiResult<SpotifyRecentlyPlayedResponse>.AsFailure("Failed to deserialise the response");
        }
        
        return ApiResult<SpotifyRecentlyPlayedResponse>.AsSuccess(responseContent);
    }

    // https://developer.spotify.com/documentation/web-api/reference/get-list-users-playlists
    public async Task<ApiResult<SpotifyUserPlaylistsResponse>> GetSpotifyUserPlaylists()
    {
        var bearerToken = _appMemoryStore.SpotifyTokenStore.GetAccessToken();
    
        if (string.IsNullOrEmpty(bearerToken))
        {
            return ApiResult<SpotifyUserPlaylistsResponse>.AsFailure("No Access Token Found");
        }
    
        var userId = _appMemoryStore.SpotifyTokenStore.GetUserId();
    
        if (string.IsNullOrEmpty(userId))
        {
            return ApiResult<SpotifyUserPlaylistsResponse>.AsFailure("No User Id Stored");
        }
        
        var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.spotify.com/v1/users/{userId}/playlists");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
    
        var httpClient = _httpClientFactory.CreateClient();
        var response = await httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            return ApiResult<SpotifyUserPlaylistsResponse>.AsFailure("Non success returned from spotify api");
        }
        
        var responseContent = await response.Content.ReadFromJsonAsync<SpotifyUserPlaylistsResponse>();

        if (responseContent == null)
        {
            return ApiResult<SpotifyUserPlaylistsResponse>.AsFailure("Failed to get playlists");
        }
        
        return ApiResult<SpotifyUserPlaylistsResponse>.AsSuccess(responseContent);
        
    }
    

}