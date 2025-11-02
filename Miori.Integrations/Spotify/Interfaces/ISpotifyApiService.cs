using Miori.Models;
using Miori.Models.Spotify;

namespace Miori.Integrations.Spotify.Interfaces;

public interface ISpotifyApiService
{
    Task<Result<string>> GetSpotifyProfileIdForNewRegister(SpotifyTokenResponse tokenResponse);
    Task<SpotifyProfileDto> GetSpotifyUserData(ulong discordUserId);
}