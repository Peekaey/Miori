using ShoukoV2.Models;
using ShoukoV2.Models.Anilist;

namespace ShoukoV2.BusinessService.Interfaces;

public interface IAnilistBusinessService
{
    Task<Result<AnilistProfileDto>> GetAnilistProfile();
    Task<AnilistProfileDto> FetchAnilistProfileFromApiConcurrently();
    Task<Result<AnilistProfileDto>> GetCachedAnilistProfile();
}