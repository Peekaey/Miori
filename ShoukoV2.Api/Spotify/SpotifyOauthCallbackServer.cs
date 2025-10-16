using System.Net.Http.Json;
using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Microsoft.AspNetCore.Builder;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using ShoukoV2.Models.Configuration;
using ShoukoV2.Models.Spotify;

namespace ShoukoV2.Integrations.Spotify;

public class SpotifyOauthCallbackServer : IHostedService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<SpotifyOauthCallbackServer> _logger;
    private readonly IConfiguration _configuration;
    private readonly AppMemoryStore _memoryStore;
    private WebApplication? _app;
    private readonly string _scope = "playlist-read-private playlist-read-collaborative user-read-recently-played";
    private readonly string _endpoint = "https://accounts.spotify.com/authorize?";

    public SpotifyOauthCallbackServer(IHttpClientFactory httpClientFactory, ILogger<SpotifyOauthCallbackServer> logger, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var builder = WebApplication.CreateBuilder();

        builder.WebHost.ConfigureKestrel(options => { options.ListenAnyIP(5001); });

        _app = builder.Build();

        _app.MapGet("/callback/spotify", async (HttpContext context) =>
        {
            var code = context.Request.Query["code"].ToString();
            var error = context.Request.Query["error"].ToString();

            if (!string.IsNullOrEmpty(error))
            {
                _logger.LogError("Spotify OAuth error: {Error}", error);
                return Results.Content(
                    "<h1>Authentication failed</h1><p>You can close this window.</p>",
                    "text/html");
            }

            if (string.IsNullOrEmpty(code))
            {
                return Results.Content(
                    "<h1>Invalid request</h1><p>No authorization code received.</p>",
                    "text/html");
            }
            
            var clientId = _configuration["Spotify:ClientId"]!;
            var clientSecret = _configuration["Spotify:ClientSecret"]!;
            var redirectUri = _configuration["Spotify:RedirectUri"] ?? "http://127.0.0.1:5001/callback/spotify";
            
            var credentials = Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
            

            var httpClient = _httpClientFactory.CreateClient();
            var tokenRequest = new Dictionary<string, string>
            {
                ["grant_type"] = "authorization_code",
                ["code"] = code,
                ["redirect_uri"] = redirectUri,
                ["scope"] = _scope
            };
            
            var request = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token")
            {
                Content = new FormUrlEncodedContent(tokenRequest)
            };
            
            request.Headers.Add("Authorization", $"Basic {credentials}");
            request.Content.Headers.ContentType = 
                new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");

            var response = await httpClient.SendAsync(request);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error during Spotify OAuth token exchange: {StatusCode} - {Content}",
                    response.StatusCode, errorContent);
                return Results.Content(
                    "<h1>Token exchange failed</h1><p>Please try again.</p>",
                    "text/html");
            }

            var tokenResponse = await response.Content.ReadFromJsonAsync<SpotifyTokenResponse>();

            return Results.Content(
                "<h1>Success!</h1><p>You've been authenticated with Spotify. You can close this window.</p>",
                "text/html");

        });
        _ = _app.RunAsync(cancellationToken);
        Console.WriteLine("Spotify Oauth callback server started on http://127.0.0.1:5001");
        await Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_app != null)
        {
            await _app.StopAsync(cancellationToken);
            await _app.DisposeAsync();
        }
    }
}