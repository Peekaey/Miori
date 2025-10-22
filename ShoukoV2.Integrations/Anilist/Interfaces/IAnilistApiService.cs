using ShoukoV2.Models;
using ShoukoV2.Models.Anilist;

namespace ShoukoV2.Integrations.Anilist.Interfaces;

public interface IAnilistApiService
{
    Task<Result<AnilistViewerResponse>> GetAnilistProfileInfo();
    Task<Result<AnilistViewerStatisticsResponse>> GetAnilistProfileStatistics();
}