using ShoukoV2.Models.Anilist;
using ShoukoV2.Models.Spotify;

namespace ShoukoV2.Models;

public class AggregateProfileDto
{
    public DiscordRichPresenceSocketDto DiscordProfileData { get; set; }
    public AnilistProfileDto AnilistProfileData { get; set; }
    public SpotifyProfileDto SpotifyProfileData { get; set; }
}