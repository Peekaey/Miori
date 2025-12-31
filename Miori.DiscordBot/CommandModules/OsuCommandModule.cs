using Miori.Helpers;
using Miori.Models;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace Miori.DiscordBot.CommandModules;

public class OsuCommandModule : ApplicationCommandModule<ApplicationCommandContext>
{
    private readonly ILogger<OsuCommandModule> _logger;
    private readonly ApplicationCommandService<ApplicationCommandContext> _applicationCommandService;
    private readonly IConfiguration _configuration;
    private readonly IOauthHelpers  _oauthHelpers;

    public OsuCommandModule(ILogger<OsuCommandModule> logger,
        ApplicationCommandService<ApplicationCommandContext> applicationCommandService,
        IConfiguration configuration, IOauthHelpers oauthHelpers)
    {
        _logger = logger;
        _applicationCommandService = applicationCommandService;
        _configuration = configuration;
        _oauthHelpers = oauthHelpers;
    }

    [SlashCommand("authenticate-with-osu", "authenticate with Osu oauth")]
    public async Task SendOsuAuthenticationLink()
    {
        var contextWrapper = new ContextWrapper(Context.Interaction, "authenticate-with-spotify");
        
        _logger.LogInteractionStart(contextWrapper.CommandName, contextWrapper.UserName, contextWrapper.UserId, contextWrapper.InteractionId, 
            contextWrapper.InteractionTimeUtc,contextWrapper.GuildId);

        try
        {
            await Context.Interaction.SendResponseAsync(InteractionCallback.DeferredMessage(MessageFlags.Ephemeral));
            
            var authUrl = _oauthHelpers.GenerateOsuAuthorisationUrl(Context.User.Id);
                
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
                Content = "Unexpected error when running authenticate with Osu command",
                Flags = MessageFlags.Ephemeral
            });
        }
    }

}