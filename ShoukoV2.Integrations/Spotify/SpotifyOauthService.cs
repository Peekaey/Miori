using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ShoukoV2.Integrations.Spotify.Interfaces;

namespace ShoukoV2.Integrations.Spotify;

// https://developer.spotify.com/documentation/web-api/tutorials/code-flow
public class SpotifyOauthService : ISpotifyOauthService
{
    private readonly ILogger<SpotifyOauthService> _logger;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly string scope = "playlist-read-private playlist-read-collaborative user-read-recently-played";
    private readonly string endpoint = "https://accounts.spotify.com/authorize?";
    
    public SpotifyOauthService(ILogger<SpotifyOauthService> logger, HttpClient httpClient, IConfiguration configuration)
    {
        _logger = logger;
        _httpClient = httpClient;
        _configuration = configuration;
    }

    // public string GenerateAuthUrl(ulong discordUserId)
    // {
    //     
    //     
    // }
}