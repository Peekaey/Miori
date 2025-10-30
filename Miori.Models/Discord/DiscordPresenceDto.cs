using NetCord.Gateway;

namespace Miori.Models.Discord;

public class DiscordPresenceDto
{
    // TODO clean up the response object to only include necessary data
    public Presence? Presence { get; set; }
}