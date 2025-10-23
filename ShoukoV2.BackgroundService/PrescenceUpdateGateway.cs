using Microsoft.Extensions.Logging;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using ShoukoV2.Api.SignalR;
using ShoukoV2.BusinessService.Interfaces;
using ShoukoV2.DiscordBot.Internal.Interfaces;
using ShoukoV2.Helpers.Discord;
using ShoukoV2.Models;

namespace ShoukoV2.DiscordBot;

public class PrescenceUpdateGateway : IPresenceUpdateGatewayHandler 
{
    private readonly ILogger<PrescenceUpdateGateway> _logger;
    private readonly IDiscordBusinessService  _discordBusinessService;
    private readonly IDiscordPresenceHub  _discordPresenceHub;
    
    public PrescenceUpdateGateway(ILogger<PrescenceUpdateGateway> logger, IDiscordBusinessService discordBusinessService,
        IDiscordPresenceHub  discordPresenceHub)
    {
        _logger = logger;
        _discordBusinessService = discordBusinessService;
        _discordPresenceHub = discordPresenceHub;
    }
    
    // When user presence activity changes
    // HandleAsync is automatically executed with the newest presence data
    // Map the presence data to a dto and then broadcast it via signalR
    public async ValueTask HandleAsync(Presence arg)
    {
        _logger.LogInformation("{}", arg);
        DiscordRichPresenceSocketDto dto = arg.MapToDto();
        await _discordPresenceHub.SendMessage(dto);
    }
}

 