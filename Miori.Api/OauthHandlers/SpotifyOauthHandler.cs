using System.Net.Http.Json;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Miori.BusinessService.Interfaces;
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
    private readonly IOauthHelpers _oauthHelpers;
    private readonly ISpotifyBusinessService  _spotifyBusinessService;
    
    public SpotifyOauthHandler(IHttpClientFactory httpClientFactory, IConfiguration configuration, 
        ILogger<SpotifyOauthHandler> logger, AppMemoryStore appMemoryStore, IOauthHelpers oauthHelpers, ISpotifyBusinessService spotifyBusinessService)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
        _appMemoryStore = appMemoryStore;
        _oauthHelpers = oauthHelpers;
        _spotifyBusinessService = spotifyBusinessService;
    }
    
    public async Task<OAuthCallbackResult> HandleCallbackAsync(string? code, string? error, string? state)
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

            if (string.IsNullOrEmpty(state))
            {
                _logger.LogApplicationError(DateTime.UtcNow, "Invalid request - No state received");
                return OAuthCallbackResult.Error("Invalid request - No state received");
            }
            
            bool isStateValid = _oauthHelpers.TryValidateState(state, out var discordUserId);

            if (isStateValid == false)
            {
                _logger.LogApplicationError(DateTime.UtcNow, "Invalid request - State invalid or expired");
                return OAuthCallbackResult.Error("Invalid request - State invalid or expired");
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

            if (tokenResponse == null)
            {
                _logger.LogApplicationError(DateTime.UtcNow, "Failed to parse SpotifyOauthHandler token response");
                return OAuthCallbackResult.Error("Failed to parse token response");
            }
            
            var registerResult = await _spotifyBusinessService.RegisterNewSpotifyUser(discordUserId, tokenResponse);

            if (registerResult.ResultOutcome != ResultEnum.Success)
            {
                _logger.LogApplicationError(DateTime.UtcNow, "Error when attempting to retrieve spotify profile data for register");
                return OAuthCallbackResult.Error("Error when attempting to retrieve spotify profile data for register");
            }

            _logger.LogApplicationMessage(DateTime.UtcNow, "Spotify access token & user registered");
            return OAuthCallbackResult.Success();
            
        }
        catch (Exception ex)
        {
            _logger.LogApplicationException(DateTime.UtcNow, ex, "SpotifyOauthHandler");
            return OAuthCallbackResult.Error(ex.Message);
        }
    }
}