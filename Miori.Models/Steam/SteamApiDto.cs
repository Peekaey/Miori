namespace Miori.Models.Steam;

public class SteamApiDto
{
    public SteamPlayerSummariesResponse PlayerSummaries { get; set; }
    public SteamRecentGamesResponse RecentGames { get; set; }
}