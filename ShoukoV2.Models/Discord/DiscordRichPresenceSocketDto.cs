using ShoukoV2.Models.Enums;

namespace ShoukoV2.Models;

public class DiscordRichPresenceSocketDto
{
    public ulong Uuid { get; set; }
    public ulong GuildId { get; set; }
    public List<DiscordActivity> DiscordActivities { get; set; }
}

public class DiscordActivity
{
    // UserActivity.Name
    // Also Makes to "Spotify"
    public string ActivityName { get; set; }
    // UserActivity.State "Editing foobar.fs
    // Also makes to Artist
    public string State { get; set; }
    // UserActivity.Details - Foobar
    // Also makes to Song Name
    public string Details { get; set; }
    // UserActivity.Assets.LargeText
    // Album Name
    public string LargeText { get; set; }
    // UserActivity.Assets.LargeImageId
    // Album Art
    public string LargeImageId { get; set; }
    // UserActivity.Assets.SmallImageId
    public string SmallImageId { get; set; }
    // UserActivity.Assets.SmallText
    public string SmallText { get; set; }
    
    // UserActivity.Timestamps.StartTime
    // Start of song
    public DateTime TimeStampStartUtc { get; set; }
    // UserActivity.Timestamps.EndTime
    // End of song
    public DateTime TimeStampEndUtc { get; set; }
    
    // UserActivity.Type "Playing"
    public string ActivityType { get; set; }
    // UserActivity.CreatedAt "Activity Start"
    public DateTime CreatedAtUtc { get; set; }
    
    
    
    
}

