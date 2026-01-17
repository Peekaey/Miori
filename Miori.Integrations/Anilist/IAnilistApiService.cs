using Miori.Models;
using Miori.Models.Anilist;

namespace Miori.Integrations.Anilist.Interfaces;

public interface IAnilistApiService
{
    Task<Result<int>> GetAnilistProfileIdForNewRegister(AnilistTokenResponse tokenResponse);
    Task<AnilistResponseDto> FetchAnilistDataFromApiConcurrently(ulong discordUserId);
}