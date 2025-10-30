using System.Net.Http.Json;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Miori.Helpers;
using Miori.Integrations.Spotify.Interfaces;
using Miori.Models.Configuration;
using Miori.Models.Enums;
using Miori.Models.Results;
using Miori.Models.Spotify;

namespace Miori.Api.OauthHandlers;

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
        try
        {
            if (!string.IsNullOrEmpty(error))
            {
                string errorMessage = $"Spotify Oauth error: {error}";
                _logger.LogApplicationError(DateTime.UtcNow, errorMessage);
                return OAuthCallbackResult.Error("Authentication failed");
            }

            if (string.IsNullOrEmpty(code))
            {
                _logger.LogApplicationError(DateTime.UtcNow, "Invalid request - No authorisation code received");
                return OAuthCallbackResult.Error("Invalid request - No authorisation code received");
            }

            var clientId = _configuration["SpotifyClientId"];
            var clientSecret = _configuration["SpotifyClientSecret"];
            var redirectUri = _configuration["SpotifyRedirectUri"];
            var scope = _configuration["SpotifyScope"];
            
            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            {
                _logger.LogApplicationError(DateTime.UtcNow, "Spotify credentials not configured");
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
                string errorMessage = $"Token exchange failed: {response.StatusCode} - {errorContent}";
                _logger.LogApplicationError(DateTime.UtcNow, errorMessage);
                return OAuthCallbackResult.Error("Token exchange failed");
            }

            var tokenResponse = await response.Content.ReadFromJsonAsync<SpotifyTokenResponse>();

            if (tokenResponse != null)
            {
                _appMemoryStore.SpotifyTokenStore.RegisterAccessToken(tokenResponse.access_token,
                    tokenResponse.refresh_token, TokenType.Bearer);

                // TODO hacky -> Implement a proper background service in the future
                await _spotifyApiService.RegisterSpotifyUserId();

                _logger.LogApplicationMessage(DateTime.UtcNow, "Spotify access token & user registered");
                return OAuthCallbackResult.Success();
            }

            _logger.LogApplicationError(DateTime.UtcNow, "Failed to parse SpotifyOauthHandler token response");
            return OAuthCallbackResult.Error("Failed to parse token response");
        }
        catch (Exception ex)
        {
            _logger.LogApplicationException(DateTime.UtcNow, ex, "SpotifyOauthHandler");
            return OAuthCallbackResult.Error(ex.Message);
        }
    }
}