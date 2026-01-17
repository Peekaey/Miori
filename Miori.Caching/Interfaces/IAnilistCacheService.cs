using Miori.Models;
using Miori.Models.Anilist;

namespace Miori.BusinessService;

public interface IAnilistCacheService
{
    Task<Result<AnilistResponseDto>> GetCachedAnilistProfile(ulong discordUserId);
}