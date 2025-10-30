using Miori.Helpers;
using Miori.Models;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace Miori.DiscordBot.CommandModules;

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
        var contextWrapper = new ContextWrapper(Context.Interaction, "authenticate-with-spotify");
        _logger.LogInteractionStart(contextWrapper.CommandName, contextWrapper.UserName, contextWrapper.UserId, contextWrapper.InteractionId, 
            contextWrapper.InteractionTimeUtc,contextWrapper.GuildId);
        
        try
        {
            await Context.Interaction.SendResponseAsync(InteractionCallback.DeferredMessage());

            _logger.LogInteractionEnd(contextWrapper.CommandName, contextWrapper.UserName, contextWrapper.UserId,
                contextWrapper.InteractionId, contextWrapper.InteractionTimeUtc, contextWrapper.GuildId);
            
            await Context.Interaction.SendFollowupMessageAsync(new InteractionMessageProperties
            {
                Content = $"Pong!"
            });
        }
        catch (Exception e)
        {
            _logger.LogInteractionException(e,contextWrapper.CommandName, contextWrapper.UserName, contextWrapper.UserId,
                contextWrapper.InteractionId, contextWrapper.InteractionTimeUtc, contextWrapper.GuildId);
            
            await Context.Interaction.SendFollowupMessageAsync(new InteractionMessageProperties
            {
                Content = "Unexpected error occured when running ping command"
            });
        }
    }
}