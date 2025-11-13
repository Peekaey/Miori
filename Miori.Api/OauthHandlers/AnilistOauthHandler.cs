using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Miori.BusinessService.Interfaces;
using Miori.Helpers;
using Miori.Integrations.Anilist.Interfaces;
using Miori.Models.Anilist;
using Miori.Models.Configuration;
using Miori.Models.Enums;
using Miori.Models.Results;
using Miori.TokenStore;

namespace Miori.Api.OauthHandlers;

public class AnilistOauthHandler : IAnilistOauthHandler
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration  _configuration;
    private readonly ILogger<AnilistOauthHandler> _logger;
    private readonly IOauthHelpers _oauthHelpers;
    private readonly IAnilistBusinessService  _anilistBusinessService;

    public AnilistOauthHandler(IHttpClientFactory httpClientFactory, IConfiguration configuration,
        ILogger<AnilistOauthHandler> logger, IOauthHelpers oauthHelpers,
        IAnilistBusinessService anilistBusinessService)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
        _oauthHelpers = oauthHelpers;
        _anilistBusinessService = anilistBusinessService;
    }

    public async Task<OAuthCallbackResult> HandleCallbackAsync(string? code, string? error, string? state)
    {
        try
        {
            if (!string.IsNullOrEmpty(error))
            {
                string errorMessage = $"Anilist Oauth error: {error}";
                _logger.LogApplicationError(DateTime.UtcNow, errorMessage);
                return OAuthCallbackResult.Error("Authenticated failed");
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
            var clientId = _configuration["AnilistClientId"];
            var clientSecret = _configuration["AnilistClientSecret"];
            var redirectUri = _configuration["AnilistRedirectUri"];

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
                string errorMessage = $"Token exchange failed: {response.StatusCode} - {errorContent}";
                _logger.LogApplicationError(DateTime.UtcNow, errorMessage);
                return OAuthCallbackResult.Error("Token Exchange Failed");
            }

            var tokenResponse = await response.Content.ReadFromJsonAsync<AnilistTokenResponse>();
            
            if (tokenResponse == null)
            {
                _logger.LogApplicationError(DateTime.UtcNow, "Failed to parse AnilistOauthHandler token response");
                return OAuthCallbackResult.Error("Failed to parse token response");
            }
            
            var registerResult = await _anilistBusinessService.RegisterNewAnilistUser(discordUserId, tokenResponse);

            if (registerResult.ResultOutcome != ResultEnum.Success)
            {
                _logger.LogApplicationError(DateTime.UtcNow, "Error when attempting to retrieve anilist profile data for register");
                return OAuthCallbackResult.Error("Error when attempting to retrieve anilist profile data for register");

            }
            _logger.LogApplicationMessage(DateTime.UtcNow, "Anilist access token & user registered");
            return OAuthCallbackResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogApplicationException(DateTime.UtcNow, ex, "AnilistOauthHandler");
            return OAuthCallbackResult.Error(ex.Message);
        }
    }
}