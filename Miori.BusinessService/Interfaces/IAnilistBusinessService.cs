using Miori.Models;
using Miori.Models.Anilist;

namespace Miori.BusinessService.Interfaces;

public interface IAnilistBusinessService
{
    Task<Result<AnilistProfileDto>> GetAnilistProfile();
    Task<AnilistProfileDto> FetchAnilistProfileFromApiConcurrently();
    Task<Result<AnilistProfileDto>> GetCachedAnilistProfile();
}