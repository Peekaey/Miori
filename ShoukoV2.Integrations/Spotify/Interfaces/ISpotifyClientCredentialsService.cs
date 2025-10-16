using ShoukoV2.Models;

namespace ShoukoV2.Integrations.Spotify.Interfaces;

public interface ISpotifyClientCredentialsService
{
    Task<ApiResult<string>> GetAccessToken();
}