using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ShoukoV2.Integrations.Anilist.Interfaces;
using ShoukoV2.Models;
using ShoukoV2.Models.Anilist;
using ShoukoV2.Models.Configuration;

namespace ShoukoV2.Integrations.Anilist;

public class AnilistApiService : IAnilistApiService
{
    private readonly ILogger<AnilistApiService> _logger;
    private readonly IConfiguration  _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AppMemoryStore _appMemoryStore;
    private readonly string _apiEndpoint = "https://graphql.anilist.co";

    // TODO figure out a better way to store these graphql queries
    private readonly string _currentUserQuery =
        @"
        query {
          Viewer {
            id
            name
            avatar {
              large
            }
            bannerImage
          }
        }";

    private readonly string _currentUserStatistics =
        @"
        query {
          Viewer {
            id
            name
            avatar {
              large
            }
            bannerImage
            statistics {
              anime {
                count
                meanScore
                volumesRead
              }
              manga {
                chaptersRead
                count
                meanScore
              }
            }
          }
        }";
    
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
            return Result<AnilistViewerResponse>.AsFailure("Bearer token not found");
        }
        
        var requestBody = new
        {
            query = _currentUserQuery,
        };
        
        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var httpClient = _httpClientFactory.CreateClient();
        
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", bearerToken);

        
        var response = await httpClient.PostAsync(_apiEndpoint, content);

        if (!response.IsSuccessStatusCode)
        {
            return Result<AnilistViewerResponse>.AsError("Unsuccessfull response from Anilist Api");
        }
        
        var responseString = await response.Content.ReadAsStringAsync();
        var responseObject = JsonSerializer.Deserialize<AnilistViewerResponse>(responseString);
        if (responseObject == null)
        {
            return Result<AnilistViewerResponse>.AsError("Failed to deserialise the response");
        }
        
        return Result<AnilistViewerResponse>.AsSuccess(responseObject);
    }
    
    public async Task<Result<AnilistViewerStatisticsResponse>> GetAnilistProfileStatistics()
    {
        var bearerToken = _appMemoryStore.AnilistTokenStore.GetAccessToken();

        if (string.IsNullOrEmpty(bearerToken))
        {
            return Result<AnilistViewerStatisticsResponse>.AsFailure("Bearer token not found");
        }
        
        var requestBody = new
        {
            query = _currentUserStatistics
        };
        
        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var httpClient = _httpClientFactory.CreateClient();
        
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", bearerToken);

        
        var response = await httpClient.PostAsync(_apiEndpoint, content);

        if (!response.IsSuccessStatusCode)
        {
            return Result<AnilistViewerStatisticsResponse>.AsError("Unsuccessful response from Anilist Api");
        }
        
        var responseString = await response.Content.ReadAsStringAsync();
        var responseObject = JsonSerializer.Deserialize<AnilistViewerStatisticsResponse>(responseString);
        if (responseObject == null)
        {
            return Result<AnilistViewerStatisticsResponse>.AsError("Failed to deserialise the response");
        }
        
        return Result<AnilistViewerStatisticsResponse>.AsSuccess(responseObject);
    }
    
    

}