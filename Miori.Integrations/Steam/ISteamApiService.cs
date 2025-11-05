using Miori.Models.Steam;

namespace Miori.Integrations.Steam;

public interface ISteamApiService
{
    Task<SteamApiResponses> FetchSteamDataFromApiConcurrently(ulong discordUserId);
    Task<ulong?> FetchUniqueSteamId(string steamId);
}