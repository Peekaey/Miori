using System.Net;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Services;
using NetCord.Hosting.Services.ApplicationCommands;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using Serilog;
using Serilog.Debugging;
using Serilog.Sinks.Grafana.Loki;
using ShoukoV2.Api;
using ShoukoV2.Api.Anilist;
using ShoukoV2.Api.SignalR;
using ShoukoV2.BackgroundService;
using ShoukoV2.BusinessService;
using ShoukoV2.BusinessService.Interfaces;
using ShoukoV2.DataService;
using ShoukoV2.DiscordBot.Internal;
using ShoukoV2.DiscordBot.Internal.Interfaces;
using ShoukoV2.Helpers.Oauth;
using ShoukoV2.Integrations.Anilist;
using ShoukoV2.Integrations.Anilist.Interfaces;
using ShoukoV2.Integrations.Spotify;
using ShoukoV2.Integrations.Spotify.Interfaces;
using ShoukoV2.Models.Configuration;

namespace ShoukoV2.DiscordBot;

class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Starting ShoukoV2...");
        
        var builder = WebApplication.CreateBuilder(args);
        
        ValidateEnvironmentVariables(builder);
        
        builder.Configuration
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", 
                optional: true, 
                reloadOnChange: true)
            .AddEnvironmentVariables();
        
        ConfigureNetCordBuilder(builder);
        ConfigureServices(builder);
        AddOauthHandlerEndpoints(builder);
        
        
        if (builder.Configuration["EnableRemoteLogging"].ToLower() == "true")
        {
            ConfigureRemoteLogging(builder);
        }
        
        var host = builder.Build();
        ConfigureNetcordHost(host);
        MapSpotifyOauthEndpoints(host);
        MapAnilistOauthEndpoints(host);
        MapAppMemoryStore(host);
        host.Run();
        Console.WriteLine("ShoukoV2 is running...");
    }
    
    static void ConfigureNetCordBuilder(WebApplicationBuilder  builder)
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

    static void ConfigureServices(WebApplicationBuilder  builder)
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

        builder.Services.AddSingleton<IAnilistBusinessService, AnilistBusinessService>();
        builder.Services.AddSingleton<IDiscordBusinessService, DiscordBusinessService>();
        builder.Services.AddSingleton<ISpotifyBusinessService, SpotifyBusinessService>();
        builder.Services.AddSingleton<IUnraidBusinessService, UnraidBusinessService>();
        
        builder.Services.AddSingleton<IDiscordGatewayService, DiscordGatewayService>();
        builder.Services.AddSingleton<IDiscordRestService, DiscordRestService>();
        builder.Services.AddSingleton<IDiscordPresenceHub, DiscordPresenceHub>();
        
        builder.Services.AddSignalR();
        builder.Services.AddControllers();

        builder.Services.AddSingleton<IOauthHelpers, OauthHelpers>();
        builder.Services.AddHybridCache();
        
        var enableCaching = builder.Configuration["EnableCaching"];

        // Only register the background service if enableCaching is enabled
        if ( enableCaching != null && enableCaching.ToLower() == "true")
        {
            builder.Services.AddSingleton<BackgroundWorkerService>();
            builder.Services.AddSingleton<IBackgroundWorkerService>(provider => provider.GetRequiredService<BackgroundWorkerService>());
            builder.Services.AddHostedService<BackgroundWorkerService>(provider => provider.GetRequiredService<BackgroundWorkerService>());
        }

    }

    // https://stackoverflow.com/questions/75365849/using-rate-limiting-in-asp-net-core-7-web-api-by-ip-address
    // Won't work if its behind cloudflare / reverse proxy
    // Need to do more research/testing
    // static void AddRateLimiting(WebApplicationBuilder builder)
    // {
    //     builder.Services.AddRateLimiter(options =>
    //     {
    //         options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, IPAddress>(context =>
    //         {
    //             IPAddress remoteIpAddress = context.Connection.RemoteIpAddress;
    //
    //             if (!IPAddress.IsLoopback(remoteIpAddress))
    //             {
    //                 return RateLimitPartition.GetTokenBucketLimiter(
    //                     remoteIpAddress!, _ => new TokenBucketRateLimiterOptions
    //                     {
    //                         TokenLimit = 10,
    //                         QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
    //                         QueueLimit = 10,
    //                         ReplenishmentPeriod = TimeSpan.FromSeconds(60),
    //                         TokensPerPeriod = 10,
    //                         AutoReplenishment = true
    //                     });
    //             }
    //
    //             return RateLimitPartition.GetNoLimiter(IPAddress.Loopback);
    //         });
    //     });
    // }
    

    static void AddOauthHandlerEndpoints(WebApplicationBuilder  builder)
    {
        var port = builder.Configuration.GetValue<int>("NetworkPort", 5001);
        
        
        builder.Services.AddSingleton<ISpotifyOauthHandler, SpotifyOauthHandler>();
        builder.Services.AddSingleton<IAnilistOauthHandler, AnilistOauthHandler>();
        builder.WebHost.ConfigureKestrel(options =>
        {
            // options.ListenAnyIP(5001);
            options.Listen(IPAddress.Any, 5001);
        });
    }

    static void ConfigureNetcordHost(WebApplication host)
    {
        host.AddModules(typeof(Program).Assembly);
        host.MapControllers();
        host.MapHub<DiscordPresenceHub>("/presencehub");
        var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
        lifetime.ApplicationStarted.Register(void () =>
        {
            try
            {
                // Execute Post Startup Utilities
                Console.WriteLine("----------ShoukoV2 Startup Complete---------");

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
    
    // No need for persistant database at this stage
    // private static void ConfigureDatabaseService(WebApplicationBuilder  builder)
    // {
    //     string postgresConnectionString = builder.Configuration["DatabaseConnectionString"];
    //     
    //     if (string.IsNullOrEmpty(postgresConnectionString))
    //     {
    //         throw new ArgumentException("Postgres connection string is not configured.", nameof(postgresConnectionString));
    //     }
    //
    //     builder.Services.AddDbContext<DataContext>(options =>
    //     {
    //         options.UseNpgsql(postgresConnectionString, npgsqlOptions =>
    //         {
    //             npgsqlOptions.MigrationsAssembly("ShoukoV2.DataService");
    //         });
    //         options.UseNpgsql(postgresConnectionString)
    //             .LogTo(Console.WriteLine, LogLevel.Information);
    //     });
    // }

    private static void MapSpotifyOauthEndpoints(WebApplication host)
    {
        host.MapGet("/callback/spotify", async (
            HttpContext context,
            [FromServices] ISpotifyOauthHandler oAuthHandler) =>
        {
            var code = context.Request.Query["code"].ToString();
            var error = context.Request.Query["error"].ToString();

            var result = await oAuthHandler.HandleCallbackAsync(code, error);
            if (result.IsSuccess)
            {
                if (host.Configuration["EnableCaching"].ToLower() == "true")
                {
                    var backgroundWorkerService = host.Services.GetService<IBackgroundWorkerService>();
                    if (backgroundWorkerService != null)
                    {
                        await backgroundWorkerService.RefreshSpotifyDataCache();
                    }
                }
                return Results.Content(OAuthResponseBuilder.BuildSuccessPage(), "text/html");
            }
            else
            {
                return Results.Content(OAuthResponseBuilder.BuildErrorPage(result.ErrorMessage!), "text/html");
            }
        });
    }

    private static void MapAnilistOauthEndpoints(WebApplication host)
    {
        host.MapGet("/callback/anilist", async (
            HttpContext context,
            [FromServices] IAnilistOauthHandler oAuthHandler) =>
        {
            var code = context.Request.Query["code"].ToString();
            var error = context.Request.Query["error"].ToString();

            var result = await oAuthHandler.HandleCallbackAsync(code, error);
            if (result.IsSuccess)
            {
                if (host.Configuration["EnableCaching"].ToLower() == "true")
                {
                    var backgroundWorkerService = host.Services.GetService<IBackgroundWorkerService>();
                    if (backgroundWorkerService != null)
                    {
                        await backgroundWorkerService.RefreshAnilistDataCache();
                    }
                }

                return Results.Content(OAuthResponseBuilder.BuildSuccessPage(), "text/html");
            }
            else
            {
                return Results.Content(OAuthResponseBuilder.BuildErrorPage(result.ErrorMessage!), "text/html");
            }
        });
    }

    private static void MapAppMemoryStore(WebApplication host)
    {
        var appMemoryStore = host.Services.GetService<AppMemoryStore>();
        if (appMemoryStore != null)
        {
            var configuration = host.Services.GetService<IConfiguration>();
            
            var enableCachingResult = configuration["EnableCaching"];

            if (enableCachingResult == null || enableCachingResult.ToLower() != "true" && enableCachingResult.ToLower() != "false")
            {
                throw new ArgumentException("EnableCaching option not specified or invalid parameter provided");
            }
            
            if (enableCachingResult.ToLower() == "true")
            {
                appMemoryStore.SetCacheOption(true);
            }
            else
            {
                appMemoryStore.SetCacheOption(false);
            }
        }
    }
    
    private static void ConfigureRemoteLogging(WebApplicationBuilder builder)
    {
        // Even though these parameters would of been validated already, double check just in case
        var lokiUrl = builder.Configuration["LokiUrl"];
        var lokiUsername = builder.Configuration["LokiUsername"];
        var lokiApiToken = builder.Configuration["LokiApiToken"];

        if (string.IsNullOrEmpty(lokiUrl) || string.IsNullOrEmpty(lokiUsername) || string.IsNullOrEmpty(lokiApiToken))
        {
            throw new ArgumentException("LokiUrl,LokiUsername and lokiApiToken must be provided if EnableRemoteLogging set to true");
        }

        builder.Host.UseSerilog((context, configuration) =>
            configuration
                .WriteTo.Console()
                .WriteTo.GrafanaLoki(
                    lokiUrl,
                    labels: new List<LokiLabel>
                    {
                        new() { Key = "app", Value = "ShoukoV2" },
                    },
                    credentials: new LokiCredentials
                    {
                        Login = lokiUsername,
                        Password =  lokiApiToken
                    }));
        
    }

    private static void TestLogging(WebApplication host)
    {
        var logger = host.Services.GetRequiredService<ILogger<Program>>();
        
        logger.LogInformation("Application starting - Test log message");
        logger.LogWarning("This is a warning message for testing");
        logger.LogError("This is an error message for testing");
    }


    public static void ValidateEnvironmentVariables(WebApplicationBuilder builder)
    {
        var configuration = builder.Configuration;
        
        var discordBotToken = configuration["DiscordBotToken"];
        var discordOwnerUserId = configuration["DiscordOwnerUserId"];
        var discordOwnerGuildId = configuration["DiscordOwnerGuildId"];

        if (string.IsNullOrEmpty(discordBotToken) || string.IsNullOrEmpty(discordOwnerUserId) ||
            string.IsNullOrEmpty(discordOwnerGuildId))
        {
            throw new ArgumentException("Discord bot token, Discord owner userId, Discord owner guildId must be provided");
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

            if (string.IsNullOrEmpty(lokiUrl) || string.IsNullOrEmpty(lokiUsername) ||
                string.IsNullOrEmpty(lokiApiToken))
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
        
        
    }
}