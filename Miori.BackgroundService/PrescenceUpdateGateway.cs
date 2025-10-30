using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Miori.Api.SignalR;
using Miori.BusinessService.Interfaces;
using Miori.Helpers;
using Miori.Models.Discord;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;

namespace Miori.BackgroundService;

public class PrescenceUpdateGateway : IPresenceUpdateGatewayHandler 
{
    private readonly ILogger<PrescenceUpdateGateway> _logger;
    private readonly IDiscordBusinessService  _discordBusinessService;
    private readonly IHubContext<DiscordPresenceHub>  _discordPresenceHub;
    private readonly string _groupName = "PresenceSubscription";

    
    public PrescenceUpdateGateway(ILogger<PrescenceUpdateGateway> logger, IDiscordBusinessService discordBusinessService,
        IHubContext<DiscordPresenceHub>  discordPresenceHub)
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
        _logger.LogApplicationMessage(DateTime.UtcNow, "Discord Presence Update Received");
        DiscordRichPresenceSocketDto dto = arg.MapToDto();
        await SendSignalRMessage(dto);
    }
    
    private async Task SendSignalRMessage(DiscordRichPresenceSocketDto message)
    {
        try
        {
            await _discordPresenceHub.Clients.Group(_groupName).SendAsync("ReceiveMessage", message);
            _logger.LogApplicationMessage(DateTime.UtcNow,$"Broadcast to group '{_groupName}' completed" );
        }
        catch (Exception ex)
        {
            _logger.LogApplicationException(DateTime.UtcNow, ex, $"Broadcast to group '{_groupName}' error");
        }
    }
}

 