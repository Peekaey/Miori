using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ShoukoV2.Integrations.Anilist;
using ShoukoV2.Integrations.Anilist.Interfaces;
using ShoukoV2.Models.Anilist;
using ShoukoV2.Models.Configuration;
using ShoukoV2.Models.Spotify;

namespace ShoukoV2.Api.Anilist;

public class AnilistOauthHandler : IAnilistOauthHandler
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration  _configuration;
    private readonly ILogger<AnilistOauthHandler> _logger;
    private readonly AppMemoryStore _appMemoryStore;
    private readonly IAnilistApiService _anilistApiService;

    public AnilistOauthHandler(IHttpClientFactory httpClientFactory, IConfiguration configuration,
        ILogger<AnilistOauthHandler> logger, AppMemoryStore appMemoryStore, IAnilistApiService  anilistApiService)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
        _appMemoryStore = appMemoryStore;
        _anilistApiService = anilistApiService;
    }

    public async Task<OAuthCallbackResult> HandleCallbackAsync(string? code, string? error)
    {
        if (!string.IsNullOrEmpty(error))
        {
            _logger.LogError("Anilist Oauth error: {}", error);
        }

        if (string.IsNullOrEmpty(code))
        {
            return OAuthCallbackResult.Error("Invalid request - No authorisation code received");
        }
        
        var clientId = _configuration["Anilist:ClientId"];
        var clientSecret = _configuration["Anilist:ClientSecret"];
        var redirectUri = _configuration["Anilist:RedirectUri"];

        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
        {
            _logger.LogError("Anilist credentials not configured");
            return OAuthCallbackResult.Error("Server configuration error");
        }

        var tokenRequest = new Dictionary<string, string>
        {
            ["client_id"] = clientId,
            ["client_secret"] = clientSecret,
            ["redirect_uri"] = redirectUri,
            ["code"] = code,
            ["grant_type"] = "authorization_code"
        };
        var jsonTokenRequest = JsonSerializer.Serialize(tokenRequest);

        var httpClient = _httpClientFactory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Post, "https://anilist.co/api/v2/oauth/token")
        {
            Content = new StringContent(jsonTokenRequest)
        };
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        
        var response = await httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Token exchange failed: {StatusCode} - {Content}", response.StatusCode, errorContent);
            return  OAuthCallbackResult.Error("Token Exchange Failed");
        }

        var tokenResponse = await response.Content.ReadFromJsonAsync<AnilistTokenResponse>();
        if (tokenResponse != null)
        {
            _appMemoryStore.AnilistTokenStore.RegisterAccessToken(tokenResponse.access_token, tokenResponse.refresh_token);
            _logger.LogInformation("Access Token" + tokenResponse.access_token);
            _logger.LogInformation("Refresh Token" + tokenResponse.refresh_token);

            await _anilistApiService.GetAnilistProfileInfo();
        }
        
        return OAuthCallbackResult.Success();
    }
}