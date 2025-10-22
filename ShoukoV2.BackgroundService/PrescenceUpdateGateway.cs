using Microsoft.Extensions.Logging;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using ShoukoV2.BusinessService.Interfaces;
using ShoukoV2.DiscordBot.Internal.Interfaces;

namespace ShoukoV2.DiscordBot;

public class PrescenceUpdateGateway : IPresenceUpdateGatewayHandler 
{
    private readonly ILogger<PrescenceUpdateGateway> _logger;
    private readonly IDiscordBusinessService  _discordBusinessService;
    
    public PrescenceUpdateGateway(ILogger<PrescenceUpdateGateway> logger, IDiscordBusinessService discordBusinessService)
    {
        _logger = logger;
        _discordBusinessService = discordBusinessService;
    }
    
    public async ValueTask HandleAsync(Presence arg)
    {
        _logger.LogInformation("{}", arg);
        var member = await _discordBusinessService.GetGuildMemberAsync(arg.GuildId, arg.User.Id);
    }
}

 