using Miori.Models;
using Miori.Models.Steam;

namespace Miori.BusinessService.Interfaces;

public interface ISteamBusinessService
{
    Task<ApiResult<SteamMappedDto>> GetSteamDataForApi(ulong steamId);
    Task<ulong?> MapSteamIdToUniqueSteamId(string steamId);
}