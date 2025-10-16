using System.Net.Http.Json;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ShoukoV2.Models.Configuration;
using ShoukoV2.Models.Spotify;

namespace ShoukoV2.Integrations.Spotify;

public class SpotifyOauthHandler : ISpotifyOauthHandler
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SpotifyOauthHandler> _logger;
    private readonly AppMemoryStore _appMemoryStore;
    private readonly string _scope = "playlist-read-private playlist-read-collaborative user-read-recently-played";
    private readonly string _endpoint = "https://accounts.spotify.com/authorize?";
    
    public SpotifyOauthHandler(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<SpotifyOauthHandler> logger, AppMemoryStore appMemoryStore)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
        _appMemoryStore = appMemoryStore;
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
            return OAuthCallbackResult.Error("Invalid request - No authorization code received");
        }

        var clientId = _configuration["Spotify:ClientId"];
        var clientSecret = _configuration["Spotify:ClientSecret"];
        var redirectUri = _configuration["Spotify:RedirectUri"];

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
            ["scope"] = _scope
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
            _appMemoryStore.SpotifyTokenStore.AccessToken = tokenResponse.Access_Token;
            _logger.LogInformation(tokenResponse.Access_Token);
            return OAuthCallbackResult.Success();
        }

        return OAuthCallbackResult.Error("Failed to parse token response");
    }
    
    public string GenerateAuthorisationUrl()
    {
        var clientId = _configuration["Spotify:ClientId"];
        var redirectUri = Uri.EscapeDataString(_configuration["Spotify:RedirectUri"]);
        
        return "https://accounts.spotify.com/authorize?" +
               $"client_id={clientId}" +
               $"&response_type=code" +
               $"&redirect_uri={redirectUri}" +
               $"&scope={Uri.EscapeDataString(_scope)}";
    }
}