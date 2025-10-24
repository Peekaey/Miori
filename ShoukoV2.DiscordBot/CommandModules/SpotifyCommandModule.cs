using System.Net.Mime;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using ShoukoV2.Helpers;
using ShoukoV2.Helpers.Oauth;
using ShoukoV2.Integrations.Spotify;
using ShoukoV2.Models;

namespace ShoukoV2.Interactions.Interactions;

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

            var ownerDiscordId = _configuration["DiscordOwnerUserId"];
            var interactionOwnerId = Context.Interaction.User.Id;

            if (ownerDiscordId != interactionOwnerId.ToString())
            {
                _logger.LogInteractionEnd(contextWrapper.CommandName, contextWrapper.UserName, contextWrapper.UserId,
                    contextWrapper.InteractionId, contextWrapper.InteractionTimeUtc, contextWrapper.GuildId);
                
                await Context.Interaction.SendFollowupMessageAsync(new InteractionMessageProperties
                {
                    Content = "This user is not allowed to execute this command",
                    Flags = MessageFlags.Ephemeral
                });
            }
            else
            {
                var authUrl = _oauthHelpers.GenerateSpotifyAuthorisationUrl();
                
                _logger.LogInteractionEnd(contextWrapper.CommandName, contextWrapper.UserName, contextWrapper.UserId,
                    contextWrapper.InteractionId, contextWrapper.InteractionTimeUtc, contextWrapper.GuildId);
                
                await Context.Interaction.SendFollowupMessageAsync(new InteractionMessageProperties
                {
                    Content = authUrl,
                    Flags = MessageFlags.Ephemeral
                });
            }
            
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