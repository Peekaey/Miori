using System.Net.Http.Json;
using System.Xml.Linq;
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
    
    public async Task<SteamApiResponses> FetchSteamDataFromApiConcurrently(ulong discordUserId)
    {
        var steamApiDto = new SteamApiResponses();

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
    
    public async Task<ulong?> FetchUniqueSteamId(string steamId)
    {
        try
        {
            if (steamId.Length == 17 && ulong.TryParse(steamId, out ulong uniqueSteamId))
            {
                // Unique Steam Identifiers are 17 digits - if it parses to ulong than its probably legit - can just shortcut and ping their profile to test
                var ping = await GetSteamProfileByUniqueIdentifier(uniqueSteamId.ToString());

                if (ping == true)
                {
                    return uniqueSteamId;
                    // True = profile legit - otherwise lets assume its a vanity url and try another way
                }
            }
            
            var httpClient = _httpClientFactory.CreateClient();
            
            // Custom Vanity Url
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://steamcommunity.com/id/{steamId}?xml=1");

            var httpClientResponse = await httpClient.SendAsync(request);

            if (!httpClientResponse.IsSuccessStatusCode)
            {
                // Means that the profile is private or does not exist
                return null;
            }

            var responseString = await httpClientResponse.Content.ReadAsStringAsync();

            if (string.IsNullOrEmpty(responseString))
            {
                // Means we somehow didn't the valid XML from steam
                return null;
            }

            XDocument doc = XDocument.Parse(responseString);

            string? steamId64 = doc.Descendants("steamID64").FirstOrDefault()?.Value;

            if (string.IsNullOrEmpty(steamId64))
            {
                // We weren't able to find the steamID64 key which is strange, usually this is caused by the provided steam Id already being the unique steam profile id
                // and returning an error
                // If it reaches this point after already checking the unique steam id in the first place, probably a lost cause |> therefore return null
                return null;
            }
            
            ulong? steamIdLong = ulong.Parse(steamId64);

            if (steamIdLong == null || steamIdLong.HasValue == false)
            {
                // Didn't get a valid string that converts to long - meaning its not a steam unique Id. Junk
                return null;
            }
            return steamIdLong;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationException(DateTime.UtcNow, ex, "Exception when getting Steam Unique ID");
            return null;
        }
    }

    public async Task<bool> GetSteamProfileByUniqueIdentifier(string uniqueIdentifier)
    {
        // A valid unique Identifier is 17 digits long
        var request = new HttpRequestMessage(HttpMethod.Get, $"https://steamcommunity.com/profiles/{uniqueIdentifier}");
        var httpClient = _httpClientFactory.CreateClient();

        var httpClientResponse = await httpClient.SendAsync(request);

        if (!httpClientResponse.IsSuccessStatusCode)
        {
            // Means it wasn't a hit - profile doesn't exist
            return false;
        }

        return true;
    }
    

    
}