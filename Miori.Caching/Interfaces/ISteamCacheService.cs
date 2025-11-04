using Miori.Models;
using Miori.Models.Steam;

namespace Miori.Cache.Interfaces;

public interface ISteamCacheService
{
    Task<Result<SteamApiDto>> GetCachedSteamData(ulong steamId);
    
}