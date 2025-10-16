using ShoukoV2.Models.Enums;

namespace ShoukoV2.Models.Configuration;

public class AppMemoryStore
{
    public SpotifyTokenStore SpotifyTokenStore { get; set; }
}

public class SpotifyTokenStore
{
    // By Default The Token Only Lasts 1 Hour
    public DateTime IssuedAtUtc { get; set; }
    public string AccessToken { get; set; }
    public TokenType TokenType { get; set; } = TokenType.Bearer;
}