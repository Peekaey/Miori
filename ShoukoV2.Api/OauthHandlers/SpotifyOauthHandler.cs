using System.Net.Http.Json;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ShoukoV2.Integrations.Spotify.Interfaces;
using ShoukoV2.Models.Configuration;
using ShoukoV2.Models.Enums;
using ShoukoV2.Models.Spotify;

namespace ShoukoV2.Integrations.Spotify;

public class SpotifyOauthHandler : ISpotifyOauthHandler
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SpotifyOauthHandler> _logger;
    private readonly AppMemoryStore _appMemoryStore;
    private readonly ISpotifyApiService  _spotifyApiService;
    
    public SpotifyOauthHandler(IHttpClientFactory httpClientFactory, IConfiguration configuration, 
        ILogger<SpotifyOauthHandler> logger, AppMemoryStore appMemoryStore, ISpotifyApiService spotifyApiService)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
        _appMemoryStore = appMemoryStore;
        _spotifyApiService = spotifyApiService;
    }
    
    public async Task<OAuthCallbackResult> HandleCallbackAsync(string? code, string? error)
    {
        if (!string.IsNullOrEmpty(error))
        {
            _logger.LogError("Spotify OAuth error: {Error}", error);
            return OAuthCallbackResult.Error("Authentication failed");
        }

        if (string.IsNullOrEmpty(code))
        {
            return OAuthCallbackResult.Error("Invalid request - No authorisation code received");
        }

        var clientId = _configuration["Spotify:ClientId"];
        var clientSecret = _configuration["Spotify:ClientSecret"];
        var redirectUri = _configuration["Spotify:RedirectUri"];
        var scope = _configuration["Spotify:Scope"];
        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
        {
            _logger.LogError("Spotify credentials not configured");
            return OAuthCallbackResult.Error("Server configuration error");
        }

        var credentials = Convert.ToBase64String(
            Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));

        var httpClient = _httpClientFactory.CreateClient();
        
        var tokenRequest = new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["code"] = code,
            ["redirect_uri"] = redirectUri,
            ["scope"] = scope
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token")
        {
            Content = new FormUrlEncodedContent(tokenRequest)
        };
        
        request.Headers.Add("Authorization", $"Basic {credentials}");
        request.Content.Headers.ContentType = 
            new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");

        var response = await httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Token exchange failed: {StatusCode} - {Content}",
                response.StatusCode, errorContent);
            return OAuthCallbackResult.Error("Token exchange failed");
        }

        var tokenResponse = await response.Content.ReadFromJsonAsync<SpotifyTokenResponse>();
        
        if (tokenResponse != null)
        {
            _appMemoryStore.SpotifyTokenStore.RegisterAccessToken(tokenResponse.access_token, tokenResponse.refresh_token, TokenType.Bearer);
            
            // TODO hacky -> Implement a proper background service in the future
            await _spotifyApiService.RegisterSpotifyUserId();
            
            return OAuthCallbackResult.Success();
        }

        return OAuthCallbackResult.Error("Failed to parse token response");
    }
}