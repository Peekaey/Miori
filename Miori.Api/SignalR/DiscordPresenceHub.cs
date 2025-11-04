using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Miori.Helpers;

namespace Miori.Api.SignalR;

public class DiscordPresenceHub : Hub
{
    private readonly ILogger<DiscordPresenceHub> _logger;
    private readonly string _groupName = "PresenceSubscription";
    private readonly IConfiguration  _configuration;
    
    public DiscordPresenceHub(ILogger<DiscordPresenceHub> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task JoinGroup()
    {
        var httpContext = Context.GetHttpContext();
        var something = "";
    }

    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        
        var enableApiKey = _configuration.GetValue<bool>("EnableApiKey");

        if (enableApiKey == true)
        {
            var apiKey = _configuration.GetValue<string>("ApiKey");

            if (!httpContext.Request.Headers.TryGetValue("X-API-KEY", out var providedKey))
            {
                await Clients.Caller.SendAsync("ConnectionError", new 
                { 
                    error = "Incorrect Api Key Provided",
                    code = "INVALID_API_KEY"
                });
                await Task.Delay(100);
                Context.Abort();
                return;
            }
            
        }
        
        
        // Validate that a userId was provided to subscribe to, if not then disconnect them
        if (!httpContext.Request.Headers.TryGetValue("userId", out var userId))
        {
            await Clients.Caller.SendAsync("ConnectionError", new 
            { 
                error = "No userId provided",
                code = "MISSING_USER_ID"
            });
            await Task.Delay(100);
            Context.Abort();
            return;
        }
        
        // Next we check which group they want to subscribe to
        if (httpContext.GetEndpoint().DisplayName == "/socket/dp")
        {
            // Means that they connected from the discord presence endpoint and wants to subscribe to discord activity updates
            var groupName = $"/dp?id={userId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            _logger.LogInformation("Client {ConnectionId} subscribed to presence updates for user {UserId}", Context.ConnectionId, userId);
            
        } 
        
        // if (httpContext.GetEndpoint().DisplayName == "/socket/all")
        // {
        //     // Means that they connected from the all endpoint and wants to subscribe to discord activity updates + others
        //     // Check if they also provided a steamId
        //     var steamId = httpContext.Request.Headers["steamId"].ToString();
        //     var groupName = $"/all?id={userId}";
        //     if (!string.IsNullOrEmpty(steamId))
        //     {
        //         // Cache this connection for the steam key -- Append to groupName
        //     }
        //     await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        //     _logger.LogInformation("Client {ConnectionId} subscribe to all updates for user {UserId}", Context.ConnectionId, userId);
        // }
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        _logger.LogInformation("Client {ConnectionId} disconnected", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}
