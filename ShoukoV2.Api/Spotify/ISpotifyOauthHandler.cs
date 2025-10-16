namespace ShoukoV2.Integrations.Spotify;

public interface ISpotifyOauthHandler
{
    string GenerateAuthorisationUrl();
}