using ShoukoV2.Models.Spotify;

namespace ShoukoV2.Integrations.Spotify;

public interface ISpotifyOauthHandler
{
    string GenerateAuthorisationUrl();
    Task<OAuthCallbackResult> HandleCallbackAsync(string? code, string? error);
}