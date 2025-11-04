using System.Text.Json.Serialization;

namespace Miori.Models.Steam;

public class SteamRecentGamesResponse
{
    [JsonPropertyName("response")]
    public SteamRecentGamesData Response { get; set; }
}

public class SteamRecentGamesData
{
    [JsonPropertyName("total_count")]
    public int TotalCount { get; set; }

    [JsonPropertyName("games")]
    public List<SteamGame> Games { get; set; }
}

public class SteamGame
{
    [JsonPropertyName("appid")]
    public int AppId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("playtime_2weeks")]
    public int Playtime2Weeks { get; set; }

    [JsonPropertyName("playtime_forever")]
    public int PlaytimeForever { get; set; }

    [JsonPropertyName("img_icon_url")]
    public string ImgIconUrl { get; set; }

    [JsonPropertyName("playtime_windows_forever")]
    public int PlaytimeWindowsForever { get; set; }

    [JsonPropertyName("playtime_mac_forever")]
    public int PlaytimeMacForever { get; set; }

    [JsonPropertyName("playtime_linux_forever")]
    public int PlaytimeLinuxForever { get; set; }

    [JsonPropertyName("playtime_deck_forever")]
    public int PlaytimeDeckForever { get; set; }
}