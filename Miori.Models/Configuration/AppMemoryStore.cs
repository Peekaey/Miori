using System.Collections.Concurrent;
using Miori.Models.Enums;

namespace Miori.Models.Configuration;

public class AppMemoryStore
{
    // Key is the Discord User Id
    public ConcurrentDictionary<ulong, SpotifyToken> SpotifyTokens { get; set; } = new();
    
    public bool HasSpotifyToken(ulong discordUserId) => SpotifyTokens.ContainsKey(discordUserId);

    public bool TryGetSpotifyToken(ulong discordUserId, out SpotifyToken token) => SpotifyTokens.TryGetValue(discordUserId, out token);

    public void AddOrUpdateSpotifyToken(ulong discordUserId, SpotifyToken token) => SpotifyTokens[discordUserId] = token;

    public bool RemoveSpotifyToken(ulong discordUserId) => SpotifyTokens.TryRemove(discordUserId, out _);

    public IEnumerable<ulong> GetAllSpotifyUsers() => SpotifyTokens.Keys;
    
    public ConcurrentDictionary<ulong,AnilistToken> AnilistTokens { get; set; } = new();
    
    public bool HasAnilistToken(ulong discordUserId) => AnilistTokens.ContainsKey(discordUserId);

    public bool TryGetAnilistToken(ulong discordUserId, out AnilistToken token) => AnilistTokens.TryGetValue(discordUserId, out token);

    public void AddOrUpdateAnilistToken(ulong discordUserId, AnilistToken token) => AnilistTokens[discordUserId] = token;

    public bool RemoveAnilistToken(ulong discordUserId) => AnilistTokens.TryRemove(discordUserId, out _);

    public IEnumerable<ulong> GetAllAnilistUsers() => AnilistTokens.Keys;
    
    
}

public class AnilistToken
{
    // RefreshToken functionality does not currently work and AccessToken is valid for 1 year
    // Technically do not need to capture refresh token
    public ulong DiscordUserId { get; set; }
    public string AnilistUserId { get; set; }
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public DateTime IssuedAtUtc { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public TokenType TokenType { get; set; } = TokenType.Bearer;
    
    public bool IsExpired => DateTime.UtcNow >= ExpiresAtUtc;
    public bool NeedsRefresh => DateTime.UtcNow >= ExpiresAtUtc.AddDays(-1); // Access Token Lasts 1 Year, Refresh 1 Day Before
    public TimeSpan TimeUntilExpiration => ExpiresAtUtc - DateTime.UtcNow;

    public static AnilistToken Create(ulong discordUserId, string anilistUserId, string accessToken,
        string refreshToken, TokenType tokenType = TokenType.Bearer)
    {
        var now = DateTime.UtcNow;
        return new AnilistToken
        {
            DiscordUserId = discordUserId,
            AnilistUserId = anilistUserId,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            IssuedAtUtc = now,
            ExpiresAtUtc = now.AddYears(1),
            TokenType = tokenType
        };
    }

    public AnilistToken WithRefreshedToken(string newRefreshToken)
    {
        var now = DateTime.UtcNow;
        
        return new AnilistToken
        {
            DiscordUserId = DiscordUserId,
            AnilistUserId = AnilistUserId,
            AccessToken = newRefreshToken,
            RefreshToken = newRefreshToken,
            IssuedAtUtc = now,
            ExpiresAtUtc = now.AddYears(1),
            TokenType = TokenType.Bearer
        };
    }
    
}
public class SpotifyToken
{
    // By Default The Token Only Lasts 1 Hour
    public ulong DiscordUserId { get; set; }
    public string SpotifyUserId { get; set; }
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public DateTime IssuedAtUtc { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public TokenType TokenType { get; set; } = TokenType.Bearer;
    
    public bool IsExpired => DateTime.UtcNow >= ExpiresAtUtc;
    public bool NeedsRefresh => DateTime.UtcNow >= ExpiresAtUtc.AddMinutes(-5); // Refresh 5 min before expiry
    public TimeSpan TimeUntilExpiration => ExpiresAtUtc - DateTime.UtcNow;

    public static SpotifyToken Create(ulong discordUserId, string spotifyUserId, string accessToken,
        string refreshToken, TokenType tokenType = TokenType.Bearer)
    {
        var now = DateTime.UtcNow;
        return new SpotifyToken
        {
            DiscordUserId = discordUserId,
            SpotifyUserId = spotifyUserId,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            IssuedAtUtc = now,
            ExpiresAtUtc = now.AddHours(1),
            TokenType = tokenType
        };
    }

    public SpotifyToken WithRefreshedToken(string newRefreshToken)
    {
        var now = DateTime.UtcNow;

        return new SpotifyToken
        {
            DiscordUserId = DiscordUserId,
            SpotifyUserId = newRefreshToken,
            AccessToken = AccessToken,
            RefreshToken = newRefreshToken,
            IssuedAtUtc = now,
            ExpiresAtUtc = now.AddHours(1),
            TokenType = TokenType
        };
    }
}

public class OsuToken
{
    // By Default The Token Lasts 24 Hours
    
    public ulong DiscordUserId { get; set; }
    public string OsuUserId { get; set; }
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public DateTime IssuedAtUtc { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public TokenType TokenType { get; set; } = TokenType.Bearer;
    
    public bool IsExpired => DateTime.UtcNow >= ExpiresAtUtc;
    public bool NeedsRefresh => DateTime.UtcNow >= ExpiresAtUtc.AddMinutes(-5); // Refresh 5 min before expiry
    public TimeSpan TimeUntilExpiration => ExpiresAtUtc - DateTime.UtcNow;

    public static OsuToken Create(ulong discordUserId, string osuUserId, string accessToken,
        string refreshToken, TokenType tokenType = TokenType.Bearer)
    {
        var now = DateTime.UtcNow;
        return new OsuToken
        {
            DiscordUserId = discordUserId,
            OsuUserId = osuUserId,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            IssuedAtUtc = now,
            ExpiresAtUtc = now.AddHours(1),
            TokenType = tokenType
        };
    }

    public OsuToken WithRefreshedToken(string newRefreshToken)
    {
        var now = DateTime.UtcNow;

        return new OsuToken
        {
            DiscordUserId = DiscordUserId,
            OsuUserId = newRefreshToken,
            AccessToken = AccessToken,
            RefreshToken = newRefreshToken,
            IssuedAtUtc = now,
            ExpiresAtUtc = now.AddHours(1),
            TokenType = TokenType
        };
    }
}