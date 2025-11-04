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
using Miori.Models.Spotify;

namespace Miori.Integrations.Anilist;

public class AnilistApiService : IAnilistApiService
{
    private readonly ILogger<AnilistApiService> _logger;
    private readonly IConfiguration  _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AppMemoryStore _appMemoryStore;
    private readonly string _apiEndpoint = "https://graphql.anilist.co";
    
    public AnilistApiService(ILogger<AnilistApiService> logger, IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        AppMemoryStore appMemoryStore)
    {
        _logger = logger;
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _appMemoryStore = appMemoryStore;
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
    
    public async Task<AnilistProfileDto> FetchAnilistDataFromApi(ulong discordUserId)
    {

        // Don't handle any errors, let it bubble up and handle
        var anilistProfileWithStatistics = await GetAnilistProfileWithStatistics(discordUserId);
        return new AnilistProfileDto
        {
            AnilistProfileResponse = anilistProfileWithStatistics.Data
        };
    }

    // https://docs.anilist.co/guide/auth/
    // Refresh Tokens are not currently supported. Once a token expires, you will need to re-authenticate your users
    // Access Tokens are long-lived. They will remain valid for 1 year from the time they are issued.
    // Therefore we do not need to build token refresh functionality for the time being
    public async Task<Result<AnilistProfileResponse>> GetAnilistProfileWithStatistics(ulong discordUserId)
    {
        try
        {
            var isTokenExist = _appMemoryStore.TryGetAnilistToken(discordUserId, out var token);

            var requestBody = new
            {
                query = AnilistQueries._currentUserStatistics
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var httpClient = _httpClientFactory.CreateClient();

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

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

}