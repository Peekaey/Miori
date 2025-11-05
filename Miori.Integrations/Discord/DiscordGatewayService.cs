using Microsoft.Extensions.Logging;
using NetCord.Gateway;

namespace Miori.Integrations.Discord;

public class DiscordGatewayService : IDiscordGatewayService
{
    private readonly GatewayClient  _gatewayClient;
    private readonly ILogger<DiscordGatewayService> _logger;

    public DiscordGatewayService(GatewayClient gatewayClient, ILogger<DiscordGatewayService> logger)
    {
        _gatewayClient = gatewayClient;
        _logger = logger;
    }

    public async Task<Presence?> GetUserPresenceAsync(ulong guildId, ulong userId)
    {
        var guild = _gatewayClient.Cache.Guilds.GetValueOrDefault(guildId);

        if (guild == null)
        {
            return null;
        }
        return guild.Presences.GetValueOrDefault(userId);
        
    }

    public async Task<int?> GetGuiltUserCountAsync(ulong guildId)
    {
        var guild = _gatewayClient.Cache.Guilds.GetValueOrDefault(guildId);
    
        if (guild == null)
        {
            return null;
        }

        var botId = this._gatewayClient.Id;
        var mutableMembers = guild.Users.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        foreach (var member in mutableMembers)
        {
            if (member.Key == botId)
            {
                mutableMembers.Remove(member.Key);
            }
        }
        return mutableMembers.Count;
    
    }
    
    
}