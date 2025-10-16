using System.Net.NetworkInformation;
using Microsoft.Extensions.Logging;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace ShoukoV2.Interactions.Interactions;

public class PingCommandModule : ApplicationCommandModule<ApplicationCommandContext>
{
    private readonly ILogger<PingCommandModule> _logger;
    private readonly ApplicationCommandService<ApplicationCommandContext> _commandService;


    public PingCommandModule(ILogger<PingCommandModule> logger, ApplicationCommandService<ApplicationCommandContext> commandService)
    {
        _logger = logger;
        _commandService = commandService;
    }

    [SlashCommand("ping", "replies with pong!")]
    public async Task SendPingAsync()
    {
        try
        {
            await Context.Interaction.SendResponseAsync(InteractionCallback.DeferredMessage());
            
            await Context.Interaction.SendFollowupMessageAsync(new InteractionMessageProperties
            {
                Content = $"Pong!"
            });
        }
        catch (Exception e)
        {
            await Context.Interaction.SendFollowupMessageAsync(new InteractionMessageProperties
            {
                Content = "Unexpected error occured when running ping command"
            });
        }
    }
}