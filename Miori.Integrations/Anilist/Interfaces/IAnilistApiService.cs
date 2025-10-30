using Miori.Models;
using Miori.Models.Anilist;

namespace Miori.Integrations.Anilist.Interfaces;

public interface IAnilistApiService
{
    Task<Result<AnilistViewerResponse>> GetAnilistProfileInfo();
    Task<Result<AnilistViewerStatisticsResponse>> GetAnilistProfileStatistics();
}