using System.Text.Json.Serialization;
using Miori.Models.Anilist;
using Miori.Models.Discord;
using Miori.Models.Osu;
using Miori.Models.Spotify;
using Miori.Models.Steam;

namespace Miori.Models;

public class AggregateProfileDto
{
    [JsonPropertyName("DiscordUserData")]
    public DiscordMappedDto DiscordUserData { get; set; }
    [JsonPropertyName("AnilistUserData")]
    public AnilistMappedDto AnilistUserData { get; set; }
    [JsonPropertyName("SpotifyUserData")]
    public SpotifyMappedDto SpotifyUserData { get; set; }
    [JsonPropertyName("SteamUserData")]
    public SteamMappedDto  SteamUserData { get; set; }
    [JsonPropertyName("OsuUserData")]
    public OsuMappedDto OsuUserData { get; set; }
}