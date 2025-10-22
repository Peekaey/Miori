using NetCord;

namespace ShoukoV2.DiscordBot.Internal.Interfaces;

public interface IDiscordRestService
{
    Task<GuildUser?> GetGuildMemberAsync(ulong guildId, ulong uuid);
}