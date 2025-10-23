using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
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
        
        builder.Configuration
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", 
                optional: true, 
                reloadOnChange: true)
            .AddEnvironmentVariables();
        
        
        ValidateEnvironmentVariables(builder);
        
        ConfigureNetCordBuilder(builder);
        ConfigureServices(builder);
        AddOauthHandlerEndpoints(builder);
        var host = builder.Build();
        ConfigureNetcordHost(host);
        MapSpotifyOauthEndpoints(host);
        MapAnilistOauthEndpoints(host);
        MapAppMemoryStore(host);
        host.Run();
        Console.WriteLine("ShoukoV2 is running...");
    }

    static void ValidateEnvironmentVariables(WebApplicationBuilder builder)
    {
        var discordBotToken = builder.Configuration["Discord:BotToken"];
        if (string.IsNullOrEmpty(discordBotToken))
        {
            throw new ArgumentException("DiscordBotToken is required", nameof(discordBotToken));
        }
    }

    static void ConfigureNetCordBuilder(WebApplicationBuilder  builder)
    {
        IEntityToken restClientToken = new BotToken(builder.Configuration["Discord:BotToken"]);
        builder.Services.AddDiscordGateway(options =>
            {
                options.Intents = GatewayIntents.GuildMessages
                                  | GatewayIntents.GuildUsers
                                  | GatewayIntents.DirectMessages
                                  | GatewayIntents.MessageContent
                                  | GatewayIntents.Guilds
                                  | GatewayIntents.GuildPresences
                                  | GatewayIntents.GuildUsers;
                options.Token = builder.Configuration["Discord:BotToken"];
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
        
        builder.Services.AddSingleton<BackgroundWorkerService>();
        
        builder.Services.AddSingleton<IBackgroundWorkerService>(provider => provider.GetRequiredService<BackgroundWorkerService>());
        
        builder.Services.AddHostedService<BackgroundWorkerService>(provider => provider.GetRequiredService<BackgroundWorkerService>());
    }
    

    static void AddOauthHandlerEndpoints(WebApplicationBuilder  builder)
    {
        builder.Services.AddSingleton<ISpotifyOauthHandler, SpotifyOauthHandler>();
        builder.Services.AddSingleton<IAnilistOauthHandler, AnilistOauthHandler>();
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.ListenAnyIP(5001);
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
            }
            catch (Exception e)
            {
                Console.WriteLine(" Error occurred while running post startup utilities: " + e.Message);
                lifetime.StopApplication();
            }
        });

        
    }
    
    
    private static void ConfigureDatabaseService(WebApplicationBuilder  builder)
    {
        string postgresConnectionString = builder.Configuration["DatabaseConnectionString"];
        
        if (string.IsNullOrEmpty(postgresConnectionString))
        {
            throw new ArgumentException("Postgres connection string is not configured.", nameof(postgresConnectionString));
        }

        builder.Services.AddDbContext<DataContext>(options =>
        {
            options.UseNpgsql(postgresConnectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly("ShoukoV2.DataService");
            });
            options.UseNpgsql(postgresConnectionString)
                .LogTo(Console.WriteLine, LogLevel.Information);
        });
    }

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
                var backgroundWorkerService = host.Services.GetService<IBackgroundWorkerService>();
                if (backgroundWorkerService != null)
                {
                    await backgroundWorkerService.RefreshSpotifyDataCache();
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
                var backgroundWorkerService = host.Services.GetService<IBackgroundWorkerService>();
                if (backgroundWorkerService != null)
                {
                    await backgroundWorkerService.RefreshAnilistDataCache();
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
    
}