using System.Text.Json.Serialization;

namespace Miori.Models.Steam;

public class SteamMappedDto
{
    [JsonPropertyName("steamid")]
    public string SteamId { get; set; }
    [JsonPropertyName("profile_url")]
    public string ProfileUrl { get; set; }
    [JsonPropertyName("persona_name")]
    public string PersonaName { get; set; }
    [JsonPropertyName("avatar")]
    public string Avatar { get; set; }
    [JsonPropertyName("last_logoff_utc")]
    public DateTimeOffset LastLogoffUtc { get; set; }
    [JsonPropertyName("time_created_utc")]
    public DateTimeOffset TimeCreatedUtc{ get; set; }
    [JsonPropertyName("recent_games")]
    public List<SteamMappedRecentGamesDto> RecentGames { get; set; }
}

public class SteamMappedRecentGamesDto
{
    [JsonPropertyName("appid")]
    public int AppId { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("playtime_2weeks_minutes")]
    public int Playtime2Weeks { get; set; }
    [JsonPropertyName("playtime_forever_minutes")]
    public int PlaytimeForever { get; set; }
    [JsonPropertyName("img_icon_url")]
    public string ImgIconUrl { get; set; }
    [JsonPropertyName("img_header_url")]
    public string ImgHeaderUrl { get; set; }
}