using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ShoukoV2.Integrations.Spotify.Interfaces;
using ShoukoV2.Models;
using ShoukoV2.Models.Spotify;

namespace ShoukoV2.Integrations.Spotify;

public class SpotifyClientCredentialsService : ISpotifyClientCredentialsService
{
    private readonly ILogger<SpotifyClientCredentialsService> _logger;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    
    public SpotifyClientCredentialsService(ILogger<SpotifyClientCredentialsService> logger, IConfiguration configuration, HttpClient httpClient)
    {
        _logger = logger;
        _configuration = configuration;
        _httpClient = httpClient;
    }

    public async Task<ApiResult<string>> GetAccessToken()
    {
        string? clientId = _configuration["Spotify:ClientId"];
        string? clientSecret = _configuration["Spotify:ClientSecret"];
        if (clientId == null || clientSecret == null)
        {
            return ApiResult<string>.AsError("Spotify client ID or secret is not configured",
                System.Net.HttpStatusCode.InternalServerError);
        }

        var request = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token");
        var formData = new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = clientId,
            ["client_secret"] = clientSecret
        };
        
        request.Content = new FormUrlEncodedContent(formData);

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            return ApiResult<string>.AsError("Error when attempting to obtain token", response.StatusCode);
        }
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var tokenResponse = System.Text.Json.JsonSerializer.Deserialize<SpotifyTokenResponse>(responseContent);
        if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.Access_Token))
        {
            return ApiResult<string>.AsError("Invalid token response from Spotify", System.Net.HttpStatusCode.InternalServerError);
        }
        return ApiResult<string>.AsSuccess(tokenResponse.Access_Token);
    }
    

}