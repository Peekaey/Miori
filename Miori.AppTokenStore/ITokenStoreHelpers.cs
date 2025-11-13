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
}