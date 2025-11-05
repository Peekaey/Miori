using Miori.Models;
using Miori.Models.Steam;

namespace Miori.Cache.Interfaces;

public interface ISteamCacheService
{
    Task<Result<SteamApiResponses>> GetCachedSteamData(ulong steamId);
    Task<Result<ulong?>> GetCachedSteamId(string steamId);

}