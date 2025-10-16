using System.Net.Mime;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using ShoukoV2.Integrations.Spotify;

namespace ShoukoV2.Interactions.Interactions;

public class SpotifyCommandModule : ApplicationCommandModule<ApplicationCommandContext>
{
    private readonly ILogger<SpotifyCommandModule> _logger;
    private readonly ApplicationCommandService<ApplicationCommandContext> _commandService;
    private readonly IConfiguration _configuration;
    private readonly ISpotifyOauthHandler  _spotifyOauthHandler;

    private readonly string _scope = "playlist-read-private playlist-read-collaborative user-read-recently-played";

    public SpotifyCommandModule(ILogger<SpotifyCommandModule> logger,
        ApplicationCommandService<ApplicationCommandContext> commandService, IConfiguration configuration, ISpotifyOauthHandler spotifyOauthHandler)
    {
        _logger = logger;
        _commandService = commandService;
        _configuration = configuration;
        _spotifyOauthHandler = spotifyOauthHandler;
    }

    [SlashCommand("authenticate-with-spotify", "authenticate with spotify oauth")]
    public async Task SendSpotifyAuthenticationLink()
    {
        try
        {
            await Context.Interaction.SendResponseAsync(InteractionCallback.DeferredMessage());
            
            var authUrl = _spotifyOauthHandler.GenerateAuthorisationUrl();
            
            
            await Context.Interaction.SendFollowupMessageAsync(new InteractionMessageProperties
            {
                Content = authUrl,
                Flags = MessageFlags.Ephemeral
            });
        }
        catch (Exception e)
        {
            await Context.Interaction.SendFollowupMessageAsync(new InteractionMessageProperties
            {
                Content = "Unexpected error when running authenticate with spotify command"
            });
        }
    }
    
}