using Miori.Models.Steam;

namespace Miori.Integrations.Steam;

public interface ISteamApiService
{
    Task<SteamApiDto> FetchSteamDataFromApiConcurrently(ulong discordUserId);
    Task<ulong?> FetchUniqueSteamId(string steamId);
}