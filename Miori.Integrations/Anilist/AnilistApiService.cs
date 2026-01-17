using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Miori.Helpers;
using Miori.Integrations.Anilist.Interfaces;
using Miori.Models;
using Miori.Models.Anilist;
using Miori.Models.Configuration;
using Miori.Models.Enums;
using Miori.Models.Spotify;
using Miori.TokenStore;

namespace Miori.Integrations.Anilist;

public class AnilistApiService : IAnilistApiService
{
    private readonly ILogger<AnilistApiService> _logger;
    private readonly IConfiguration  _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _apiEndpoint = "https://graphql.anilist.co";
    private readonly ITokenStoreHelpers _tokenStoreHelpers;
    
    public AnilistApiService(ILogger<AnilistApiService> logger, IConfiguration configuration,
        IHttpClientFactory httpClientFactory,  ITokenStoreHelpers tokenStoreHelpers
        )
    {
        _logger = logger;
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _tokenStoreHelpers = tokenStoreHelpers;
    }

    public async Task<Result<int>> GetAnilistProfileIdForNewRegister(AnilistTokenResponse tokenResponse)
    {
        try
        {
            var requestBody = new
            {
                query = AnilistQueries._currentUserQuery,
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var httpClient = _httpClientFactory.CreateClient();

            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", tokenResponse.access_token);

            var response = await httpClient.PostAsync(_apiEndpoint, content);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogApplicationError(DateTime.UtcNow, "Unsuccessful response from Anilist API when attempting to basic profile info");
                return Result<int>.AsError("Unsuccessful response from Anilist API when attempting to basic profile info");
            }

            var responseString = await response.Content.ReadAsStringAsync();
            var responseObject = JsonSerializer.Deserialize<AnilistProfileResponse>(responseString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true});
            if (responseObject == null)
            {
                _logger.LogApplicationError(DateTime.UtcNow, "Failed to deserialise the response from the Anilist API for basic profile info");
                return Result<int>.AsError("Failed to deserialise the response from the Anilist API for basic profile info");
            }

            return Result<int>.AsSuccess(responseObject.data.viewer.id);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationException(DateTime.UtcNow,ex, "Exception when getting Anilist basic profile info");
            return Result<int>.AsError("Exception when getting Anilist basic profile info");
        }
    }
    
    public async Task<AnilistResponseDto> FetchAnilistDataFromApiConcurrently(ulong discordUserId)
    {
        var anilistDto = new AnilistResponseDto();
        // Don't handle any errors, let it bubble up and handle
        var userActivityTask = GetAnilistUserActivity(discordUserId);
        var anilistProfileWithStatisticsTask = GetAnilistProfileWithStatistics(discordUserId);
        
        await Task.WhenAll(userActivityTask, anilistProfileWithStatisticsTask);

        var userActivityResult = await userActivityTask;
        var anilistProfileWithStatisticsResult = await anilistProfileWithStatisticsTask;

        if (userActivityResult.ResultOutcome == ResultEnum.Success)
        {
            anilistDto.AnilistActivityResponse = userActivityResult.Data;
        }

        if (anilistProfileWithStatisticsResult.ResultOutcome == ResultEnum.Success)
        {
            anilistDto.AnilistProfileResponse = anilistProfileWithStatisticsResult.Data;
        }
        return anilistDto;
    }

    // https://docs.anilist.co/guide/auth/
    // Refresh Tokens are not currently supported. Once a token expires, you will need to re-authenticate your users
    // Access Tokens are long-lived. They will remain valid for 1 year from the time they are issued.
    // Therefore we do not need to build token refresh functionality for the time being
    public async Task<Result<AnilistProfileResponse>> GetAnilistProfileWithStatistics(ulong discordUserId)
    {
        try
        {
            var existingAnilistCache = await _tokenStoreHelpers.GetAnilistTokens(discordUserId);
            var requestBody = new
            {
                query = AnilistQueries._currentUserStatistics
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var httpClient = _httpClientFactory.CreateClient();

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", existingAnilistCache.AccessToken);

            var response = await httpClient.PostAsync(_apiEndpoint, content);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogApplicationError(DateTime.UtcNow, "Unsuccessful response from Anilist API when attempting to profile info with statistics");
                return Result<AnilistProfileResponse>.AsError("Unsuccessful response from Anilist API when attempting to profile info with statistics");
            }

            var responseString = await response.Content.ReadAsStringAsync();

            var responseObject = JsonSerializer.Deserialize<AnilistProfileResponse>(responseString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true});

            if (responseObject == null)
            {
                _logger.LogApplicationError(DateTime.UtcNow,
                    "Failed to deserialise the response from the Anilist API when getting profile info with statistics");
                return Result<AnilistProfileResponse>.AsError(
                    "Failed to deserialise the response from the Anilist API when getting profile info with statistics");
            }

            return Result<AnilistProfileResponse>.AsSuccess(responseObject);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationException(DateTime.UtcNow,ex, "Exception when getting anilist profile info with statistics");
            return Result<AnilistProfileResponse>.AsError("Exception when getting anilist profile info with statistics");
        }
    }

    public async Task<Result<AniListActivityResponse>> GetAnilistUserActivity(ulong discordUserId, int page = 1, int perPage = 18)
{
    try
    {
        var existingAnilistCache = await _tokenStoreHelpers.GetAnilistTokens(discordUserId);
        var requestBody = new
        {
            query = AnilistQueries._userActivityQuery,
            variables = new
            {
                userId = existingAnilistCache.AnilistUserId,
                page = page,
                perPage = perPage
            }
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var httpClient = _httpClientFactory.CreateClient();

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", existingAnilistCache.AccessToken);

        var response = await httpClient.PostAsync(_apiEndpoint, content);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogApplicationError(DateTime.UtcNow,
                "Unsuccessful response from Anilist API when attempting to get user activity");
            return Result<AniListActivityResponse>.AsError(
                "Unsuccessful response from Anilist API when attempting to get user activity");
        }

        var responseString = await response.Content.ReadAsStringAsync();

        var responseObject = JsonSerializer.Deserialize<AniListActivityResponse>(responseString,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (responseObject == null)
        {
            _logger.LogApplicationError(DateTime.UtcNow,
                "Failed to deserialise the response from the Anilist API when getting user activity");
            return Result<AniListActivityResponse>.AsError("Failed to deserialise the response from the Anilist API when getting user activity");
        }

        return Result<AniListActivityResponse>.AsSuccess(responseObject);
    }
    catch (Exception ex)
    {
        _logger.LogApplicationError(DateTime.UtcNow, "Exception when getting Anilist user activity");
        return Result<AniListActivityResponse>.AsError("Exception when getting Anilist user activity");
    }
}

}