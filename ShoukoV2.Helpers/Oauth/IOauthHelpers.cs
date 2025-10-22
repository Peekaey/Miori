namespace ShoukoV2.Helpers.Oauth;

public interface IOauthHelpers
{
    string GenerateAnilistAuthorisationUrl();
    string GenerateSpotifyAuthorisationUrl();
}