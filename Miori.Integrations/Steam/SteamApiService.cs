using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Miori.Helpers;
using Miori.Models;
using Miori.Models.Configuration;
using Miori.Models.Enums;
using Miori.Models.Steam;

namespace Miori.Integrations.Steam;

public class SteamApiService : ISteamApiService
{
    private readonly ILogger<SteamApiService> _logger;
    private readonly IConfiguration _configuration;
    private readonly AppMemoryStore _appMemoryStore;
    private readonly IHttpClientFactory _httpClientFactory;

    public SteamApiService(ILogger<SteamApiService> logger, IConfiguration configuration, AppMemoryStore appMemoryStore,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _configuration = configuration;
        _appMemoryStore = appMemoryStore;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<SteamApiDto> FetchSteamDataFromApiConcurrently(ulong discordUserId)
    {
        var steamApiDto = new SteamApiDto();

        var steamPlayerSummariesTask = GetPlayerSummaries(discordUserId);
        var steamRecentlyPlayedTask = GetRecentlyPlayedGames(discordUserId);
        
        await Task.WhenAll(steamPlayerSummariesTask, steamRecentlyPlayedTask);

        var steamPlayerSummariesResult = await steamPlayerSummariesTask;
        var steamRecentlyPlayedResult = await steamRecentlyPlayedTask;

        if (steamPlayerSummariesResult.ResultOutcome == ResultEnum.Success)
        {
            steamApiDto.PlayerSummaries = steamPlayerSummariesResult.Data;
        }

        if (steamRecentlyPlayedResult.ResultOutcome == ResultEnum.Success)
        {
            steamApiDto.RecentGames = steamRecentlyPlayedResult.Data;   
        }
        return steamApiDto;
    }

    private async Task<Result<SteamPlayerSummariesResponse>> GetPlayerSummaries(ulong steamId)
    {
        try
        {
            var apiKey = _configuration.GetValue<string>("SteamApiKey");
            var request = new HttpRequestMessage(HttpMethod.Get,
                "https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/" +
                "?key=" + apiKey + "&steamids=" + steamId);
            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogApplicationError(DateTime.UtcNow,
                    "Unsuccessful response from Steam Api when attempting to get Steam Player Summaries");
                return Result<SteamPlayerSummariesResponse>.AsError(
                    "Unsuccessful response from Steam Api when attempting to get Steam Player Summaries");
            }

            var responseContent = await response.Content.ReadFromJsonAsync<SteamPlayerSummariesResponse>();

            if (responseContent == null)
            {
                _logger.LogApplicationError(DateTime.UtcNow,
                    "Failed to deserialise Steam response for PlayerSummaries");
                return Result<SteamPlayerSummariesResponse>.AsError("Failed to deserialise Steam response for PlayerSummaries");
            }

            return Result<SteamPlayerSummariesResponse>.AsSuccess(responseContent);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationException(DateTime.UtcNow, ex, "Exception when getting Steam Player Summaries");
            return Result<SteamPlayerSummariesResponse>.AsError("Exception when getting Steam Player Summaries");
        }
    }

    private async Task<Result<SteamRecentGamesResponse>> GetRecentlyPlayedGames(ulong steamId)
    {
        try
        {
            var apiKey = _configuration.GetValue<string>("SteamApiKey");
            var request = new HttpRequestMessage(HttpMethod.Get, 
                "https://api.steampowered.com/IPlayerService/GetRecentlyPlayedGames/v0001/" +
                                                                 "?key=" + apiKey + "&steamid=" + steamId + "&format=json");
            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogApplicationError(DateTime.UtcNow,
                    "Unsuccessful response from Steam Api when attempting to get Steam Recently Played Games");
                return Result<SteamRecentGamesResponse>.AsError(
                    "Unsuccessful response from Steam Api when attempting to get Steam Recently Played Games");
            }

            var responseContent = await response.Content.ReadFromJsonAsync<SteamRecentGamesResponse>();

            if (responseContent == null)
            {
                _logger.LogApplicationError(DateTime.UtcNow, "Failed to deserialise Steam response for Recently Played Games");
                return Result<SteamRecentGamesResponse>.AsError("Failed to deserialise Steam response for Recently Played Games");
            }

            return Result<SteamRecentGamesResponse>.AsSuccess(responseContent);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationException(DateTime.UtcNow, ex, "Exception when getting Steam Recently Played Games");
            return Result<SteamRecentGamesResponse>.AsError("Exception when getting Steam Recently Played Games");
        }
    }
    
    

    
}