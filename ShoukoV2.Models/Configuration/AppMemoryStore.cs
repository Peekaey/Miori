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
    public DateTime IssuedAtUtc { get; set; }
    public string AccessToken { get; set; }
    public TokenType TokenType { get; set; } = TokenType.Bearer;
    public string SpotifyId { get; set; }
}