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
        DiscordMappedDto dto = arg.MapToDto();
        await SendPresenceSocketMessage(dto, arg.User.Id);
    }
    

    private async Task SendPresenceSocketMessage(DiscordMappedDto message, ulong userId)
    {
        await SendDiscordPresenceEndpointSocketMessage(message, userId);
    }

    // private async Task SendAllEndpointSocketMessage(DiscordRichPresenceSocketDto message, ulong userId)
    // {
    //     var allGroup = $"/all?id={userId}";
    //
    // }
    private async Task SendDiscordPresenceEndpointSocketMessage(DiscordMappedDto message, ulong userId)
    {
        var rpGroup = $"/dp?id={userId}";
        try
        {
            await _discordPresenceHub.Clients.Group(rpGroup).SendAsync("ReceiveMessage", message);
            _logger.LogApplicationMessage(DateTime.UtcNow,$"Broadcast to group '{rpGroup}' completed" );
        }
        catch (Exception ex)
        {
            _logger.LogApplicationException(DateTime.UtcNow, ex, $"Broadcast to group '{rpGroup}' error");
        }
    }
}

 