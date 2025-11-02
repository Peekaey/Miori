using Miori.Models;
using Miori.Models.Spotify;

namespace Miori.Cache;

public interface ISpotifyCacheService
{
    Task<Result<SpotifyProfileDto>> GetCachedSpotifyProfile(ulong discordUserId);
}