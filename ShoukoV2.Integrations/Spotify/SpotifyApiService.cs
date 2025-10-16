using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ShoukoV2.Integrations.Spotify.Interfaces;
using ShoukoV2.Models;
using ShoukoV2.Models.Configuration;
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
        if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.Access_Token))
        {
            return ApiResult<string>.AsError("Invalid token response from Spotify", System.Net.HttpStatusCode.InternalServerError);
        }
        return ApiResult<string>.AsSuccess(tokenResponse.Access_Token);
    }

    public async Task<ApiResult<SpotifyMeResponse>> GetSpotifyProfileInfo()
    {
        var bearerToken = _appMemoryStore.SpotifyTokenStore.AccessToken;

        if (string.IsNullOrEmpty(bearerToken))
        {
            return ApiResult<SpotifyMeResponse>.AsFailure("No Access Token Found");
        }

        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.spotify.com/v1/me")
        {
            Headers = { Authorization = new AuthenticationHeaderValue($"Bearer {bearerToken}") }
        };
        
        
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
    

}