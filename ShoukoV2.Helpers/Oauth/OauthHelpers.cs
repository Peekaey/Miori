using Microsoft.Extensions.Configuration;

namespace ShoukoV2.Helpers.Oauth;

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
        var clientId = _configuration["Anilist:ClientId"];
        var redirectUri = Uri.EscapeDataString(_configuration["Anilist:RedirectUri"]);
        
        return "https://anilist.co/api/v2/oauth/authorize?" +
               "client_id=" + clientId +
               "&redirect_uri=" + redirectUri
               + "&response_type=code";
    }
    // https://developer.spotify.com/documentation/web-api/tutorials/code-flow
    public string GenerateSpotifyAuthorisationUrl()
    {
        var clientId = _configuration["Spotify:ClientId"];
        var redirectUri = Uri.EscapeDataString(_configuration["Spotify:RedirectUri"]);
        
        return "https://accounts.spotify.com/authorize?" +
               $"client_id={clientId}" +
               $"&response_type=code" +
               $"&redirect_uri={redirectUri}" +
               $"&scope={Uri.EscapeDataString(_configuration["Spotify:Scope"])}";
    }
}