using Miori.Models;
using Miori.Models.Osu;

namespace Miori.BusinessService.Interfaces;

public interface IOsuBusinessService
{
    Task<BasicResult> RegisterNewOsuUser(ulong discordUserId, OsuTokenResponse tokenResponse);
    Task<ApiResult<OsuMappedDto>> GetOsuProfileForApi(ulong discordUserId);
}