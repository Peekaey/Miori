using NetCord;

namespace Miori.Integrations.Discord;

public interface IDiscordRestService
{
    Task<GuildUser?> GetGuildMemberAsync(ulong guildId, ulong uuid);
}