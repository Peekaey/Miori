using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using ShoukoV2.Helpers;
using ShoukoV2.Models;

namespace ShoukoV2.Api.SignalR;

public class DiscordPresenceHub : Hub
{
    private readonly ILogger<DiscordPresenceHub> _logger;
    private readonly string _groupName = "PresenceSubscription";
    
    public DiscordPresenceHub(ILogger<DiscordPresenceHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var connectionId = Context.ConnectionId;
        _logger.LogApplicationMessage(DateTime.UtcNow, $"OnConnectedAsync: {connectionId}");
        // Only need a single group for DRP
        await Groups.AddToGroupAsync(Context.ConnectionId, _groupName);
        await base.OnConnectedAsync();
    }
    
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogApplicationMessage(DateTime.UtcNow, $"OnDisconnectedAsync: {Context.ConnectionId}");
        await base.OnDisconnectedAsync(exception);
    }
}