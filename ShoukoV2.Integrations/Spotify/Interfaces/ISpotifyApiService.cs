using ShoukoV2.Models;
using ShoukoV2.Models.Spotify;

namespace ShoukoV2.Integrations.Spotify.Interfaces;

public interface ISpotifyApiService
{
    Task<Result<string>> GetAccessToken();
    Task<Result<SpotifyMeResponse>> GetSpotifyProfileInfo();
    Task RegisterSpotifyUserId();
    Task<Result<SpotifyRecentlyPlayedResponse>> GetSpotifyUserRecentlyPlayed(int limit = 10);
    Task<Result<SpotifyUserPlaylistsResponse>> GetSpotifyUserPlaylists();
    Task ValidateAndRefreshToken();
}