using Miori.Models;
using Miori.Models.Configuration;

namespace Miori.TokenStore;

public interface ITokenStoreHelpers
{
    Task<BasicResult> AddOrUpdateSpotifyToken(ulong discordUserId, SpotifyToken spotifyToken);
    Task<BasicResult> AddOrUpdateAnilistToken(ulong discordUserId, AnilistToken anilistToken);
    Task<BasicResult> RemoveSpotifyToken(ulong discordUserId);
    Task<BasicResult> RemoveAnilistToken(ulong discordUserId);
    Task<SpotifyToken?> GetSpotifyTokens(ulong discordUserId);
    Task<AnilistToken?> GetAnilistTokens(ulong discordUserId);
    Task<OsuToken?> GetOsuTokens(ulong discordUserId);
    Task<BasicResult> AddOrUpdateOsuToken(ulong discordUserId, OsuToken osuToken);
    Task<BasicResult> RemoveOsuToken(ulong discordUserId);
}