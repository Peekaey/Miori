using Microsoft.AspNetCore.Mvc;
using Miori.Api;
using Miori.Api.OauthHandlers;
using Miori.Api.SignalR;
using Miori.BackgroundService;
using Miori.BusinessService;
using Miori.BusinessService.Interfaces;
using Miori.Cache;
using Miori.Cache.Interfaces;
using Miori.Helpers;
using Miori.Integrations.Anilist;
using Miori.Integrations.Anilist.Interfaces;
using Miori.Integrations.Discord;
using Miori.Integrations.Spotify;
using Miori.Integrations.Spotify.Interfaces;
using Miori.Integrations.Steam;
using Miori.Models.Configuration;
using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Services;
using NetCord.Hosting.Services.ApplicationCommands;
using NetCord.Rest;
using Serilog;
using Serilog.Sinks.Grafana.Loki;

namespace Miori.DiscordBot;

public static class Configuration
{
    public static void ConfigureNetCordBuilder(WebApplicationBuilder  builder)
    {
        IEntityToken restClientToken = new BotToken(builder.Configuration["DiscordBotToken"]);
        builder.Services.AddDiscordGateway(options =>
            {
                options.Intents = GatewayIntents.GuildMessages
                                  | GatewayIntents.GuildUsers
                                  | GatewayIntents.DirectMessages
                                  | GatewayIntents.MessageContent
                                  | GatewayIntents.Guilds
                                  | GatewayIntents.GuildPresences
                                  | GatewayIntents.GuildUsers;
                options.Token = builder.Configuration["DiscordBotToken"];
            })
            .AddApplicationCommands()
            .AddGatewayHandler<PrescenceUpdateGateway>()
            .AddGatewayHandlers(typeof(Program).Assembly)
            .AddSingleton<RestClient>(sp => new RestClient(restClientToken));
    }
    
    public static void ConfigureServices(WebApplicationBuilder  builder)
    {
        Console.WriteLine("Configuring Services...");
        builder.Services.AddLogging(logger =>
        {
            logger.ClearProviders();
            logger.AddConsole();
            logger.AddDebug();
        });
        
        builder.Services.AddSingleton<IDiscordRestService, DiscordRestService>();
        builder.Services.AddSingleton<AppMemoryStore>();
        builder.Services.AddHttpClient();
        
        builder.Services.AddSingleton<ISpotifyApiService, SpotifyApiService>();
        builder.Services.AddSingleton<IAnilistApiService, AnilistApiService>();
        builder.Services.AddSingleton<ISteamApiService, SteamApiService>();

        builder.Services.AddSingleton<IAnilistBusinessService, AnilistBusinessService>();
        builder.Services.AddSingleton<IDiscordBusinessService, DiscordBusinessService>();
        builder.Services.AddSingleton<ISpotifyBusinessService, SpotifyBusinessService>();
        builder.Services.AddSingleton<IAggregateBusinessService, AggregateBusinessService>();
        builder.Services.AddSingleton<ISteamBusinessService, SteamBusinessService>();
        
        builder.Services.AddSingleton<IAnilistCacheService, AnilistCacheService>();
        builder.Services.AddSingleton<ISpotifyCacheService, SpotifyCacheService>();
        builder.Services.AddSingleton<ISteamCacheService, SteamCacheService>();
        
        builder.Services.AddSingleton<IDiscordGatewayService, DiscordGatewayService>();
        builder.Services.AddSingleton<IDiscordRestService, DiscordRestService>();

        builder.Services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = true;
            options.KeepAliveInterval = TimeSpan.FromMinutes(15);
            options.ClientTimeoutInterval = TimeSpan.FromMinutes(1);
        });
        
        builder.Services.AddControllers();

        builder.Services.AddSingleton<IOauthHelpers, OauthHelpers>();
        
        var enableCaching = builder.Configuration["EnableCaching"];
        builder.Services.AddHybridCache();
        
        if ( enableCaching != null && enableCaching.ToLower() == "true")
        {
            Console.WriteLine("Caching is enabled...");
        }
    }
        
    public static void AddOauthHandlerEndpoints(WebApplicationBuilder  builder)
    {
        var port = builder.Configuration.GetValue<int>("NetworkPort", 5001);
        builder.Services.AddSingleton<ISpotifyOauthHandler, SpotifyOauthHandler>();
        builder.Services.AddSingleton<IAnilistOauthHandler, AnilistOauthHandler>();
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.ListenAnyIP(port);
        });
    }
    
    public static void ConfigureHost(WebApplication host)
    {
        host.AddModules(typeof(Program).Assembly);
        host.MapControllers();
        
        host.MapHub<DiscordPresenceHub>("/socket/dp");
        // host.MapHub<DiscordPresenceHub>("/socket/all");
        var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
        lifetime.ApplicationStarted.Register(void () =>
        {
            try
            {
                // Execute Post Startup Utilities
                Console.WriteLine("----------Miori Startup Complete---------");

                if (host.Configuration["EnableRemoteLogging"].ToLower() == "true")
                {
                    TestLogging(host);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(" Error occurred while running post startup utilities: " + e.Message);
                lifetime.StopApplication();
            }
        });
    }

    public static void ConfigureMinimalApiEndpoints(WebApplication host)
    {
        host.UseStatusCodePages(async context =>
        {
            var response = context.HttpContext.Response;

            if (response.StatusCode == 404)
            {
                response.ContentType = "application/json";
                await response.WriteAsJsonAsync(new
                {
                    message = "Route does not exist",
                    path = context.HttpContext.Request.Path.Value
                });
            }
        });

        host.MapGet("/", async (HttpContext ctx) =>
        {
            var discordGatewayService = ctx.RequestServices.GetRequiredService<IDiscordGatewayService>();
            var iConfig = ctx.RequestServices.GetRequiredService<IConfiguration>();
            var memberCount = await discordGatewayService.GetGuiltUserCountAsync(iConfig.GetValue<ulong>("DiscordOwnerGuildId"));
            var response = ctx.Response;
            response.ContentType = "application/json";
            await response.WriteAsJsonAsync(new
            {
                message = $"Miori provides Discord presence & various userdata from other platforms via a convenient REST Api or SignalR. See: https://github.com/Peekaey/Miori",
                monitored_members = $"{memberCount}",
            });
        });
    }
    public static void TestLogging(WebApplication host)
    {
        var logger = host.Services.GetRequiredService<ILogger<Program>>();
        
        logger.LogInformation("Application starting - RemoteLogging has been enable");
        logger.LogWarning("This is a warning message for testing");
        logger.LogError("This is an error message for testing");
    }
    
    public static void ValidateEnvironmentVariables(WebApplicationBuilder builder)
    {
        var configuration = builder.Configuration;
        
        var discordBotToken = configuration["DiscordBotToken"];
        var discordOwnerGuildId = configuration["DiscordOwnerGuildId"];

        if (string.IsNullOrEmpty(discordBotToken)  || string.IsNullOrEmpty(discordOwnerGuildId))
        {
            throw new ArgumentException("Discord bot token, Discord guildId must be provided");
        }
        
        var spotifyClientId = configuration["SpotifyClientId"];
        var spotifyClientSecret = configuration["SpotifyClientSecret"];
        var spotifyRedirectUri = configuration["SpotifyRedirectUri"];
        var spotifyScopes = configuration["SpotifyScope"];

        if (string.IsNullOrEmpty(spotifyClientId) || string.IsNullOrEmpty(spotifyClientSecret) ||
            string.IsNullOrEmpty(spotifyRedirectUri) || string.IsNullOrEmpty(spotifyScopes))
        {
            throw new ArgumentException("Spotify Client Id, Spotify Client Secret, Spotify Redirect Uri and Scope must be provided");
        }
        
        var anilistClientId = configuration["AnilistClientId"];
        var anilistClientSecret = configuration["AnilistClientSecret"];
        var anilistRedirectUri = configuration["AnilistRedirectUri"];

        if (string.IsNullOrEmpty(anilistClientId) || string.IsNullOrEmpty(anilistClientSecret) ||
            string.IsNullOrEmpty(anilistRedirectUri))
        {
            throw new ArgumentException("Anilist Client Id, Anilist Client Secret, Anilist Redirect Uri must be provided");
        }
        
        var enableCaching = configuration["EnableCaching"];

        if (string.IsNullOrEmpty(enableCaching) ||
            enableCaching.ToLower() != "true" && enableCaching.ToLower() != "false")
        {
            throw new ArgumentException("EnableCaching option not specified or invalid parameter provided - must be provided and set to true or false");
        }
        
        var enableRemoteLogging = configuration["EnableRemoteLogging"];

        if (string.IsNullOrEmpty(enableRemoteLogging) ||
            enableRemoteLogging.ToLower() != "true" && enableRemoteLogging.ToLower() != "false")
        {
            throw new ArgumentException("EnableRemoteLogging option not specified or invalid parameter provided - must be provided and set to true or false");
        }

        if (enableRemoteLogging.ToLower() == "true")
        {
            var lokiUsername = configuration["LokiUsername"];
            var lokiApiToken = configuration["LokiApiToken"];
            var lokiUrl = configuration["LokiUrl"];

            if (string.IsNullOrEmpty(lokiUrl) || string.IsNullOrEmpty(lokiUsername) || string.IsNullOrEmpty(lokiApiToken))
            {
                throw new ArgumentException("Endpoint, Username and ApiToken must be provided for Loki Instance");
            }
        }
        
        var port = configuration["NetworkPort"];

        if (string.IsNullOrEmpty(port))
        {
            throw new ArgumentException("NetworkPort must be provided");
        }
        else
        {
            // Attempt to parse to a number - throw an exception if not successful
            if (!int.TryParse(port, out int portNumber))
            {
                throw new ArgumentException("Port must be a valid number");
            }

        }
        
        var enableApiKey = configuration["EnableApiKey"];
        if (string.IsNullOrEmpty(enableApiKey) ||
            enableApiKey.ToLower() != "true" && enableApiKey.ToLower() != "false")
        {
            throw new ArgumentException("EnableApiKey option not specified or invalid parameter provided -must be provided and set to true or false");
        }

        if (enableApiKey.ToLower() == "true")
        {
            var apiKey  = configuration["ApiKey"];

            if (string.IsNullOrEmpty(apiKey))
            {
                throw new ArgumentException("ApiKey must be provided if EnableApiKey is true");
            }
        }
        
        var stateSigningKey = configuration["StateSigningKey"];

        if (string.IsNullOrEmpty(stateSigningKey))
        {
            throw new ArgumentException("StateSigningKey must be provided");
        }
    }
    
    public static void ConfigureRemoteLogging(WebApplicationBuilder builder)
    {
        if (builder.Configuration["EnableRemoteLogging"].ToLower() == "true")
        {
            // Even though these parameters would have been validated already, double check just in case
            var lokiUrl = builder.Configuration["LokiUrl"];
            var lokiUsername = builder.Configuration["LokiUsername"];
            var lokiApiToken = builder.Configuration["LokiApiToken"];

            if (string.IsNullOrEmpty(lokiUrl) || string.IsNullOrEmpty(lokiUsername) ||
                string.IsNullOrEmpty(lokiApiToken))
            {
                throw new ArgumentException(
                    "LokiUrl,LokiUsername and lokiApiToken must be provided if EnableRemoteLogging set to true");
            }

            builder.Host.UseSerilog((context, configuration) =>
                configuration
                    .WriteTo.Console()
                    .WriteTo.GrafanaLoki(
                        lokiUrl,
                        labels: new List<LokiLabel>
                        {
                            new() { Key = "app", Value = "Miori" },
                        },
                        credentials: new LokiCredentials
                        {
                            Login = lokiUsername,
                            Password = lokiApiToken
                        }));
        }
    }

    public static void ConfigureAppSettings(WebApplicationBuilder builder)
    {
        builder.Configuration
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", 
                optional: true, 
                reloadOnChange: true)
            .AddEnvironmentVariables();
    }
    
}