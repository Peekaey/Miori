using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Miori.Helpers;
using Miori.Models;
using Miori.Models.Enums;
using Miori.Models.Osu;
using Miori.TokenStore;

namespace Miori.Integrations.Osu;

public class OsuApiService : IOsuApiService
{
    private readonly ILogger<OsuApiService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ITokenStoreHelpers _tokenStoreHelpers;

    public OsuApiService(ILogger<OsuApiService> logger, IConfiguration configuration,
        IHttpClientFactory httpClientFactory, ITokenStoreHelpers tokenStoreHelpers)
    {
        _logger = logger;
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _tokenStoreHelpers = tokenStoreHelpers;
    }

    public async Task<Result<string>> GetOsuProfileIdForNewRegister(OsuTokenResponse tokenResponse)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "https://osu.ppy.sh/api/v2/me/osu");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenResponse.access_token);

            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogApplicationError(DateTime.UtcNow, "Unsuccessful response from Osu API when attempting to get Osu profile info");
                return Result<string>.AsError("Unsuccessful response from Osu API when attempting to get Osu profile info");
            }
            var responseContent = await response.Content.ReadFromJsonAsync<OsuMeResponse>();

            if (responseContent == null)
            {
                _logger.LogApplicationError(DateTime.UtcNow, "Failed to deserialise the response from the Osu API");
                return Result<string>.AsError("Failed to deserialise the response from the Osu API");
            }

            return Result<string>.AsSuccess(responseContent.id.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogApplicationException(DateTime.UtcNow, ex, "Exception when getting Osuprofile info");
            return Result<string>.AsError("Exception when getting Osu profile info");
        }
    }

    public async Task ValidateAndRefreshToken(ulong discordUserId)
    {
        var osuSpotifyToken = await _tokenStoreHelpers.GetOsuTokens(discordUserId);

        if (osuSpotifyToken.IsExpired == false)
        {
            await RefreshAccessToken(discordUserId);
        }
    }
    // https://osu.ppy.sh/docs/#client-credentials-grant
    private async Task RefreshAccessToken(ulong discordUserId)
    {
        try
        {
            var existingOsuCache = await _tokenStoreHelpers.GetOsuTokens(discordUserId);

            var clientId = _configuration["Osu:ClientId"];
            var clientSecret = _configuration["Osu:ClientSecret"];

            var tokenRequest = new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = existingOsuCache.RefreshToken,
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret,
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://osu.ppy.sh/oauth/token")
            {
                Content = new FormUrlEncodedContent(tokenRequest)
            };
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var tokenResponse = await response.Content.ReadFromJsonAsync<OsuTokenResponse>();
                if (tokenResponse != null)
                {
                    var newRefreshToken = string.IsNullOrEmpty(tokenResponse.refresh_token)
                        ? existingOsuCache.RefreshToken
                        : tokenResponse.refresh_token;
                    var replacedTokenObject = existingOsuCache.WithRefreshedToken(newRefreshToken);
                    _tokenStoreHelpers.AddOrUpdateOsuToken(discordUserId, replacedTokenObject);

                    _logger.LogApplicationMessage(DateTime.UtcNow,
                        $"Successfully refreshed Osu token for {discordUserId} with Osu user Id : '{replacedTokenObject.OsuUserId}'");
                }
                else
                {
                    _logger.LogApplicationMessage(DateTime.UtcNow, $"Failed to refresh Osu token for {discordUserId}");
                }
            }
            else
            {
                _logger.LogApplicationError(DateTime.UtcNow,
                    "Unsuccessful response from Osu Api when attempting to refresh token");
            }
        }
        catch (Exception ex)
        {
            _logger.LogApplicationException(DateTime.UtcNow, ex, "Exception when refreshing Osu access token");
        }
    }

    public async Task<OsuProfileDto> GetOsuUserData(ulong discordUserId)
    {
        // Don't handle any errors, let it bubble up and handle
        await ValidateAndRefreshToken(discordUserId);
        return await FetchOsuDataFromApiConcurrently(discordUserId);
    }

    private async Task<OsuProfileDto> FetchOsuDataFromApiConcurrently(ulong discordUserId)
    {
        var osuProfileDto = new OsuProfileDto();
        
        var osuProfileTask = GetOsuProfileInfo(discordUserId);
        var osuRecentActivityTask = GetOsuRecentActivity(discordUserId);
        
        await Task.WhenAll(osuProfileTask, osuRecentActivityTask);

        var osuProfileResult = await osuProfileTask;
        var osuRecentActivityResult = await osuRecentActivityTask;

        if (osuProfileResult.ResultOutcome == ResultEnum.Success)
        {
            osuProfileDto.OsuProfile = osuProfileResult.Data;
        }

        if (osuRecentActivityResult.ResultOutcome == ResultEnum.Success)
        {
            osuProfileDto.RecentScores = osuRecentActivityResult.Data;
        }

        return osuProfileDto;
    }

    private async Task<Result<OsuMeResponse>> GetOsuProfileInfo(ulong discordUserId)
    {
        try
        {
            var existingOsuCache = await _tokenStoreHelpers.GetOsuTokens(discordUserId);
            var request = new HttpRequestMessage(HttpMethod.Get, "https://osu.ppy.sh/api/v2/me/osu");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", existingOsuCache.AccessToken);

            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogApplicationError(DateTime.UtcNow,
                    "Unsuccessful response from Osu API when attempting to get Osu profile info");
                return Result<OsuMeResponse>.AsError(
                    "Unsuccessful response from Osu API when attempting to get Osu profile info");
            }
            var responseString = await response.Content.ReadAsStringAsync();
            var responseContent = await response.Content.ReadFromJsonAsync<OsuMeResponse>();

            if (responseContent == null)
            {
                _logger.LogApplicationError(DateTime.UtcNow,
                    "Failed to deserialise the response from the Osu API for osu profile info");
                return Result<OsuMeResponse>.AsFailure(
                    "Failed to deserialise the response from the Osu API for osu profile info");
            }

            return Result<OsuMeResponse>.AsSuccess(responseContent);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationException(DateTime.UtcNow, ex, "Exception when getting Osu profile info");
            return Result<OsuMeResponse>.AsError("Exception when getting Osu profile info");
        }
    }

    private async Task<Result<List<OsuRecentScoreResponse>>> GetOsuRecentActivity(ulong discordUserId)
    {
        try
        {
            var existingOsuCache = await _tokenStoreHelpers.GetOsuTokens(discordUserId);
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://osu.ppy.sh/api/v2/users/{existingOsuCache.OsuUserId}/scores/recent");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", existingOsuCache.AccessToken);

            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogApplicationError(DateTime.UtcNow, "Unsuccessful response from Osu API when attempting to get Osu recent activity");
                return Result<List<OsuRecentScoreResponse>>.AsError("Unsuccessful response from Osu API when attempting to get Osu recent activity");
            }
            var responseString = await response.Content.ReadAsStringAsync();
            var responseContent = await response.Content.ReadFromJsonAsync<List<OsuRecentScoreResponse>>();

            if (responseContent == null)
            {
                _logger.LogApplicationError(DateTime.UtcNow, "Failed to deserialise the response from the Osu API for osu recent activity");
                return Result<List<OsuRecentScoreResponse>>.AsFailure("Failed to deserialise the response from the Osu API for osu recent activity");
            }

            return Result<List<OsuRecentScoreResponse>>.AsSuccess(responseContent);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationException(DateTime.UtcNow, ex, "Exception when getting Osu recent activity");
            return Result<List<OsuRecentScoreResponse>>.AsError("Exception when getting Osu recent activity");
        }
    }
    
}