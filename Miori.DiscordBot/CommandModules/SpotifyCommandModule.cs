using Miori.Helpers;
using Miori.Models;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace Miori.DiscordBot.CommandModules;

public class SpotifyCommandModule : ApplicationCommandModule<ApplicationCommandContext>
{
    private readonly ILogger<SpotifyCommandModule> _logger;
    private readonly ApplicationCommandService<ApplicationCommandContext> _commandService;
    private readonly IConfiguration _configuration;
    private readonly IOauthHelpers  _oauthHelpers;
    public SpotifyCommandModule(ILogger<SpotifyCommandModule> logger,
        ApplicationCommandService<ApplicationCommandContext> commandService, IConfiguration configuration, IOauthHelpers oauthHelpers)
    {
        _logger = logger;
        _commandService = commandService;
        _configuration = configuration;
        _oauthHelpers = oauthHelpers;
    }

    [SlashCommand("authenticate-with-spotify", "authenticate with spotify oauth")]
    public async Task SendSpotifyAuthenticationLink()
    {
        var contextWrapper = new ContextWrapper(Context.Interaction, "authenticate-with-spotify");
        
        _logger.LogInteractionStart(contextWrapper.CommandName, contextWrapper.UserName, contextWrapper.UserId, contextWrapper.InteractionId, 
            contextWrapper.InteractionTimeUtc,contextWrapper.GuildId);
        
        try
        {
            await Context.Interaction.SendResponseAsync(InteractionCallback.DeferredMessage(MessageFlags.Ephemeral));
            
            var authUrl = _oauthHelpers.GenerateSpotifyAuthorisationUrl(Context.User.Id);
                
            _logger.LogInteractionEnd(contextWrapper.CommandName, contextWrapper.UserName, contextWrapper.UserId,
                contextWrapper.InteractionId, contextWrapper.InteractionTimeUtc, contextWrapper.GuildId);
                
            await Context.Interaction.SendFollowupMessageAsync(new InteractionMessageProperties
            {
                Content = authUrl,
                Flags = MessageFlags.Ephemeral
            });
            
        }
        catch (Exception e)
        {
            _logger.LogInteractionException(e,contextWrapper.CommandName, contextWrapper.UserName, contextWrapper.UserId,
                contextWrapper.InteractionId, contextWrapper.InteractionTimeUtc, contextWrapper.GuildId);
            
            await Context.Interaction.SendFollowupMessageAsync(new InteractionMessageProperties
            {
                Content = "Unexpected error when running authenticate with spotify command",
                Flags = MessageFlags.Ephemeral
            });
        }
    }
    
}