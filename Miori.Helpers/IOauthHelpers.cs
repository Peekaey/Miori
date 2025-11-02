namespace Miori.Helpers;

public interface IOauthHelpers
{
    string GenerateAnilistAuthorisationUrl(ulong userDiscordId);
    string GenerateSpotifyAuthorisationUrl(ulong userDiscordId);
    bool TryValidateState(string state, out ulong discordUserId);
}