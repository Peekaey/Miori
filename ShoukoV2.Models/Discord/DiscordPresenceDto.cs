using NetCord.Gateway;

namespace ShoukoV2.Models.Discord;

public class DiscordPresenceDto
{
    // TODO clean up the response object to only include necessary data
    public Presence? Presence { get; set; }
}