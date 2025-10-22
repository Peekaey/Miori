using NetCord.Gateway;

namespace ShoukoV2.BackgroundService;

public interface IDiscordGatewayService
{
    Task<Presence?> GetUserPresenceAsync(ulong guildId, ulong userId);
}