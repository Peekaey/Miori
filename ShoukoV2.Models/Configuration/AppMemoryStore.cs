using ShoukoV2.Models.Enums;

namespace ShoukoV2.Models.Configuration;

public class AppMemoryStore
{
    public SpotifyTokenStore SpotifyTokenStore { get; set; } = new();
}

public class SpotifyTokenStore
{
    // By Default The Token Only Lasts 1 Hour
    // We will only capture one set of tokens/Ids as we do not currently need to store more than one persons 
    // set of tokens/data as registration will only be allowed to one user
    private DateTime AccessTokenIssuedAtUtc { get; set; }
    private string AccessToken { get; set; }
    private string RefreshToken { get; set; }
    private TokenType TokenType { get; set; } = TokenType.Bearer;
    private string SpotifyId { get; set; }

    public void RegisterAccessToken(string token, string refreshToken ,TokenType tokenType = TokenType.Bearer)
    {
        AccessTokenIssuedAtUtc = DateTime.UtcNow;
        AccessToken = token;
        TokenType = tokenType;
        RefreshToken = refreshToken;
    }

    public void RegisterSpotifyId(string spotifyId)
    {
        SpotifyId = spotifyId;
    }

    public string GetAccessToken()
    {
        return AccessToken;
    }

    public string GetRefreshToken()
    {
        return RefreshToken;
    }

    public DateTime GetTokenIssueDate()
    {
        return AccessTokenIssuedAtUtc;
    }
}