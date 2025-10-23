using ShoukoV2.Models;
using ShoukoV2.Models.Spotify;

namespace ShoukoV2.BusinessService.Interfaces;

public interface ISpotifyBusinessService
{
    Task<Result<SpotifyProfileDto>> GetSpotifyProfile();
    Task<SpotifyProfileDto> FetchSpotifyProfileFromApiConcurrently();
    Task<Result<SpotifyProfileDto>> GetCachedSpotifyProfile();
}