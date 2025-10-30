using Microsoft.Extensions.Configuration;

namespace Miori.Helpers;

public class OauthHelpers : IOauthHelpers
{
    private readonly IConfiguration  _configuration;

    public OauthHelpers(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    // https://docs.anilist.co/guide/auth/authorization-code
    public string GenerateAnilistAuthorisationUrl()
    {
        var clientId = _configuration["AnilistClientId"];
        var redirectUri = Uri.EscapeDataString(_configuration["AnilistRedirectUri"]);
        
        return "https://anilist.co/api/v2/oauth/authorize?" +
               "client_id=" + clientId +
               "&redirect_uri=" + redirectUri
               + "&response_type=code";
    }
    // https://developer.spotify.com/documentation/web-api/tutorials/code-flow
    public string GenerateSpotifyAuthorisationUrl()
    {
        var clientId = _configuration["SpotifyClientId"];
        var redirectUri = Uri.EscapeDataString(_configuration["SpotifyRedirectUri"]);
        
        return "https://accounts.spotify.com/authorize?" +
               $"client_id={clientId}" +
               $"&response_type=code" +
               $"&redirect_uri={redirectUri}" +
               $"&scope={Uri.EscapeDataString(_configuration["SpotifyScope"])}";
    }
}