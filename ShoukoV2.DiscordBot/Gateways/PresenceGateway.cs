using Microsoft.Extensions.Logging;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using ShoukoV2.DiscordBot.Internal.Interfaces;

namespace ShoukoV2.BackgroundService;

public class PresenceGateway : IPresenceUpdateGatewayHandler
{
    private readonly ILogger<PresenceGateway> _logger;
    private readonly IGuildMemberHelpers _guildMemberHelpers;
    
    public PresenceGateway(ILogger<PresenceGateway> logger, IGuildMemberHelpers guildMemberHelpers)
    {
        _logger = logger;
        _guildMemberHelpers = guildMemberHelpers;
    }
    
    public async ValueTask HandleAsync(Presence arg)
    {
        _logger.LogInformation("{}", arg);
        var member = await _guildMemberHelpers.GetGuildMemberAsync(arg.GuildId, arg.User.Id);
    }
}

