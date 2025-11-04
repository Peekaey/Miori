using Miori.Models;
using Miori.Models.Steam;

namespace Miori.BusinessService.Interfaces;

public interface ISteamBusinessService
{
    Task<ApiResult<SteamApiDto>> GetSteamDataForApi(ulong steamId);
}