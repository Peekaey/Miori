using Miori.Models;
using Miori.Models.Spotify;

namespace Miori.BusinessService.Interfaces;

public interface ISpotifyBusinessService
{
    Task<BasicResult> RegisterNewSpotifyUser(ulong discordUserId, SpotifyTokenResponse tokenResponse);
    Task<ApiResult<SpotifyMappedDto>> GetSpotifyProfileForApi(ulong discordUserId);
}