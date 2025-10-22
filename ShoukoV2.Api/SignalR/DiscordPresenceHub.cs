using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using ShoukoV2.Models;

namespace ShoukoV2.Api.SignalR;

public class DiscordPresenceHub : Hub, IDiscordPresenceHub
{
    private readonly ILogger<DiscordPresenceHub> _logger;
    private readonly string _groupName = "PresenceSubscription";
    private readonly IHubContext<DiscordPresenceHub> _hubContext;
    
    public DiscordPresenceHub(ILogger<DiscordPresenceHub> logger, IHubContext<DiscordPresenceHub> hubContext)
    {
        _logger = logger;
        _hubContext = hubContext;
    }

    public override async Task OnConnectedAsync()
    {
        var connectionId = Context.ConnectionId;
        _logger.LogInformation($"OnConnectedAsync: {connectionId}");
        
        // Only need a single group for DRP
        await Groups.AddToGroupAsync(Context.ConnectionId, _groupName);
        await base.OnConnectedAsync();
    }
    
    
    public async Task SendMessage(DiscordRichPresenceSocketDto message)
    {
        await _hubContext.Clients.Group(_groupName).SendAsync("ReceiveMessage", message);
    }

    public async Task Subscribe()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, _groupName);
        await Clients.Group(_groupName).SendAsync("UserJoined", Context.ConnectionId);
    }

    public async Task Unsubscribe()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, _groupName);
        await Clients.Group(_groupName).SendAsync("UserLeft", Context.ConnectionId);
    }
    
}