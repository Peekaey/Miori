using System.Text.Json.Serialization;

namespace Miori.Models.Discord;

public class DiscordMappedDto
{
    [JsonPropertyName("uuid")]
    public ulong Uuid { get; set; }
    [JsonPropertyName("banner_url")]
    public string BannerUrl { get; set; }
    [JsonPropertyName("avatar_url")]
    public string AvatarUrl { get; set; }
    [JsonPropertyName("global_username")]
    public string UserName { get; set; }
    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; set; }
    [JsonPropertyName("activities")]
    public List<DiscordActivityMappedDto> Activities { get; set; }
}

public class DiscordActivityMappedDto
{
    // UserActivity.Name
    // Also Makes to "Spotify"
    [JsonPropertyName("name")]
    public string ActivityName { get; set; }
    // UserActivity.State "Editing foobar.fs
    // Also makes to Artist
    [JsonPropertyName("state")]
    public string State { get; set; }
    // UserActivity.Details - Foobar
    // Also makes to Song Name
    [JsonPropertyName("details")]
    public string Details { get; set; }
    // UserActivity.Assets.LargeText
    // Album Name
    [JsonPropertyName("large_text")]
    public string LargeText { get; set; }
    // UserActivity.Assets.LargeImageId
    // Album Art
    [JsonPropertyName("large_image")]
    public string LargeImageId { get; set; }
    // UserActivity.Assets.SmallImageId
    [JsonPropertyName("small_image")]
    public string SmallImageId { get; set; }
    // UserActivity.Assets.SmallText
    [JsonPropertyName("small_text")]
    public string SmallText { get; set; }
    
    // UserActivity.Timestamps.StartTime
    // Start of song
    [JsonPropertyName("timestamp_start_utc")]
    public DateTime TimeStampStartUtc { get; set; }
    // UserActivity.Timestamps.EndTime
    // End of song
    [JsonPropertyName("timestamp_end_utc")]
    public DateTime TimeStampEndUtc { get; set; }
    
    // UserActivity.Type "Playing"
    [JsonPropertyName("activity_type")]
    public string ActivityType { get; set; }
    // UserActivity.CreatedAt "Activity Start"
    [JsonPropertyName("created_at_utc")]
    public DateTime CreatedAtUtc { get; set; }
}