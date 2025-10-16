using System.Net.Mime;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace ShoukoV2.Interactions.Interactions;

public class SpotifyCommandModule : ApplicationCommandModule<ApplicationCommandContext>
{
    private readonly ILogger<SpotifyCommandModule> _logger;
    private readonly ApplicationCommandService<ApplicationCommandContext> _commandService;
    private readonly IConfiguration _configuration;

    private readonly string _scope = "playlist-read-private playlist-read-collaborative user-read-recently-played";

    public SpotifyCommandModule(ILogger<SpotifyCommandModule> logger,
        ApplicationCommandService<ApplicationCommandContext> commandService, IConfiguration configuration)
    {
        _logger = logger;
        _commandService = commandService;
        _configuration = configuration;
    }

    [SlashCommand("authenticate-with-spotify", "authenticate with spotify oauth")]
    public async Task SendSpotifyAuthenticationLink()
    {
        try
        {
            await Context.Interaction.SendResponseAsync(InteractionCallback.DeferredMessage());
            
            var clientId = _configuration["Spotify:ClientId"];
            var redirectUri = Uri.EscapeDataString("http://127.0.0.1:5001/callback/spotify");

            var authUrl = $"https://accounts.spotify.com/authorize?" +
                          $"client_id={clientId}&" +
                          $"response_type=code&" +
                          $"redirect_uri={redirectUri}&" +
                          $"scope={Uri.EscapeDataString(_scope)}";
            
            
            await Context.Interaction.SendFollowupMessageAsync(new InteractionMessageProperties
            {
                Content = authUrl
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