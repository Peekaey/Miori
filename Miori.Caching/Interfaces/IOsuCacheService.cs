using Miori.Models;
using Miori.Models.Osu;

namespace Miori.Cache.Interfaces;

public interface IOsuCacheService
{
    Task<Result<OsuProfileDto>> GetCachedOsuProfile(ulong discordUserId);
}