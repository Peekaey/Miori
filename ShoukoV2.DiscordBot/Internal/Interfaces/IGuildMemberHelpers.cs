using NetCord;

namespace ShoukoV2.DiscordBot.Internal.Interfaces;

public interface IGuildMemberHelpers
{
    Task<GuildUser?> GetGuildMemberAsync(ulong guildId, ulong uuid);
}