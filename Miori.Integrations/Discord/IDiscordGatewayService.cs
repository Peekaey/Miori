using NetCord.Gateway;

namespace Miori.Integrations.Discord;

public interface IDiscordGatewayService
{
    Task<Presence?> GetUserPresenceAsync(ulong guildId, ulong userId);
    Task<int?> GetGuiltUserCountAsync(ulong guildId);
}