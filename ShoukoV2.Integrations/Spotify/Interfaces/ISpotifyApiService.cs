using ShoukoV2.Models;
using ShoukoV2.Models.Spotify;

namespace ShoukoV2.Integrations.Spotify.Interfaces;

public interface ISpotifyApiService
{
    Task<ApiResult<string>> GetAccessToken();
    Task<ApiResult<SpotifyMeResponse>> GetSpotifyProfileInfo();
    Task RegisterSpotifyUserId();
}