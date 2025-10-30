using Miori.Models;
using Miori.Models.Spotify;

namespace Miori.BusinessService.Interfaces;

public interface ISpotifyBusinessService
{
    Task<Result<SpotifyProfileDto>> GetSpotifyProfile();
    Task<SpotifyProfileDto> FetchSpotifyProfileFromApiConcurrently();
    Task<Result<SpotifyProfileDto>> GetCachedSpotifyProfile();
}