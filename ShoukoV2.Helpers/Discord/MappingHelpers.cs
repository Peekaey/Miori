using NetCord.Gateway;
using ShoukoV2.Models;

namespace ShoukoV2.Helpers.Discord;

public static class MappingHelpers
{
    public static DiscordRichPresenceSocketDto ToDto(this Presence presence)
    {
        return new DiscordRichPresenceSocketDto
        {
            GuildId = presence.GuildId,
            Uuid = presence.User.Id,
            DiscordActivities = presence.Activities?.Select(activity => new DiscordActivity
            {
                ActivityName = activity.Name,
                State = activity.State ?? string.Empty,
                Details = activity.Details ?? string.Empty,
                LargeText = activity.Assets?.LargeText ?? string.Empty,
                SmallText = activity.Assets?.SmallText ?? string.Empty,
                LargeImageId = activity.Assets?.LargeImageId ?? string.Empty,
                SmallImageId = activity.Assets?.SmallImageId ?? string.Empty,
                CreatedAtUtc = activity.CreatedAt.UtcDateTime,
                TimeStampStartUtc = activity.Timestamps?.StartTime?.DateTime ?? DateTime.MinValue,
                TimeStampEndUtc = activity.Timestamps?.EndTime?.DateTime ?? DateTime.MinValue,
                ActivityType = activity.Type.ToString(), 
            }).ToList() ?? new List<DiscordActivity>()
        };
    }
}