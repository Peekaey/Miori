using Miori.Models;
using Miori.Models.Anilist;

namespace Miori.BusinessService.Interfaces;

public interface IAnilistBusinessService
{
    Task<BasicResult> RegisterNewAnilistUser(ulong discordUserId, AnilistTokenResponse tokenResponse);
    Task<ApiResult<AnilistProfileDto>> GetAnilistProfileForApi(ulong discordUserId);
    
}