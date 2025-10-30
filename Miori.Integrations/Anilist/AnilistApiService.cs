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


    // https://docs.anilist.co/guide/auth/
    // Refresh Tokens are not currently supported. Once a token expires, you will need to re-authenticate your users
    // Access Tokens are long-lived. They will remain valid for 1 year from the time they are issued.
    // Therefore we do not need to build token refresh functionality
    public async Task<Result<AnilistViewerResponse>> GetAnilistProfileInfo()
    {
        var bearerToken = _appMemoryStore.AnilistTokenStore.GetAccessToken();

        if (string.IsNullOrEmpty(bearerToken))
        {
            _logger.LogApplicationError(DateTime.UtcNow,"Anilist Bearer token not found. User needs to authenticate first");
            return Result<AnilistViewerResponse>.AsFailure("Anilist Bearer token not found. User needs to authenticate first");
        }
        
        var requestBody = new
        {
            query = AnilistQueries._currentUserQuery,
        };
        
        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var httpClient = _httpClientFactory.CreateClient();
        
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", bearerToken);

        
        var response = await httpClient.PostAsync(_apiEndpoint, content);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogApplicationError(DateTime.UtcNow, "Unsuccessful response from Anilist API when attempting to basic profile info");
            return Result<AnilistViewerResponse>.AsError("Unsuccessful response from Anilist API when attempting to basic profile info");
        }
        
        var responseString = await response.Content.ReadAsStringAsync();
        var responseObject = JsonSerializer.Deserialize<AnilistViewerResponse>(responseString);
        if (responseObject == null)
        {
            _logger.LogApplicationError(DateTime.UtcNow, "Failed to deserialise the response from the Anilist API");
            return Result<AnilistViewerResponse>.AsError("Failed to deserialise the response from the Anilist API");
        }
        _logger.LogInformation("Successfully got access token from Spotify API");
        return Result<AnilistViewerResponse>.AsSuccess(responseObject);
    }
    
    public async Task<Result<AnilistViewerStatisticsResponse>> GetAnilistProfileStatistics()
    {
        var bearerToken = _appMemoryStore.AnilistTokenStore.GetAccessToken();

        if (string.IsNullOrEmpty(bearerToken))
        {
            _logger.LogApplicationError(DateTime.UtcNow,"Anilist Bearer token not found. User needs to authenticate first");
            return Result<AnilistViewerStatisticsResponse>.AsFailure("Anilist Bearer token not found. User needs to authenticate first");
        }
        
        var requestBody = new
        {
            query = AnilistQueries._currentUserStatistics
        };
        
        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var httpClient = _httpClientFactory.CreateClient();
        
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", bearerToken);

        
        var response = await httpClient.PostAsync(_apiEndpoint, content);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogApplicationError(DateTime.UtcNow, "Unsuccessful response from Anilist API when attempting to profile info");
            return Result<AnilistViewerStatisticsResponse>.AsError("Unsuccessful response from Anilist API when attempting to profile info");
        }
        
        var responseString = await response.Content.ReadAsStringAsync();
        var responseObject = JsonSerializer.Deserialize<AnilistViewerStatisticsResponse>(responseString);
        if (responseObject == null)
        {
            _logger.LogApplicationError(DateTime.UtcNow, "Failed to deserialise the response from the Anilist API");
            return Result<AnilistViewerStatisticsResponse>.AsError("Failed to deserialise the response from the Anilist API");
        }
        _logger.LogInformation("Successfully got Anilist Profile Statistics");
        return Result<AnilistViewerStatisticsResponse>.AsSuccess(responseObject);
    }
    
}