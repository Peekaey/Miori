namespace Miori.Models;

public class AggregateProfileDto
{
    public DiscordRichPresenceSocketDto DiscordProfileData { get; set; }
    public AnilistProfileDto AnilistProfileData { get; set; }
    public SpotifyProfileDto SpotifyProfileData { get; set; }
}