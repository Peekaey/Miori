using Miori.Models;
using Miori.Models.Spotify;

namespace Miori.Integrations.Spotify.Interfaces;

public interface ISpotifyApiService
{
    Task<Result<string>> GetAccessToken();
    Task<Result<SpotifyMeResponse>> GetSpotifyProfileInfo();
    Task RegisterSpotifyUserId();
    Task<Result<SpotifyRecentlyPlayedResponse>> GetSpotifyUserRecentlyPlayed(int limit = 10);
    Task<Result<SpotifyUserPlaylistsResponse>> GetSpotifyUserPlaylists();
    Task ValidateAndRefreshToken();
}