using Microsoft.AspNetCore.Builder;
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
using ShoukoV2.DataService;
using ShoukoV2.DiscordBot.Internal;
using ShoukoV2.DiscordBot.Internal.Interfaces;
using ShoukoV2.Integrations.Spotify;
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
        AddSpotifyOauthHandler(builder);
        var host = builder.Build();
        
        ConfigureNetcordHost(host);
        MapSpotifyOauthEndpoints(host);
        host.Run();
        Console.WriteLine("ShoukoV2 is running...");
    }

    static void ValidateEnvironmentVariables(WebApplicationBuilder builder)
    {
        var discordBotToken = builder.Configuration["DiscordBotToken"];
        if (string.IsNullOrEmpty(discordBotToken))
        {
            throw new ArgumentException("DiscordBotToken is required", nameof(discordBotToken));
        }
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
        
        builder.Services.AddSingleton<IGuildMemberHelpers, GuildMemberHelpers>();
        builder.Services.AddSingleton<AppMemoryStore>();
        builder.Services.AddHttpClient();
    }

    static void AddSpotifyOauthHandler(WebApplicationBuilder  builder)
    {
        builder.Services.AddSingleton<SpotifyOauthHandler>();
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.ListenAnyIP(5001);
        });
    }

    static void ConfigureNetcordHost(WebApplication host)
    {
        host.AddModules(typeof(Program).Assembly);
        
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
            SpotifyOauthHandler oAuthHandler) =>
        {
            var code = context.Request.Query["code"].ToString();
            var error = context.Request.Query["error"].ToString();

            var result = await oAuthHandler.HandleCallbackAsync(code, error);
            if (result.IsSuccess)
            {
                return Results.Content(OAuthResponseBuilder.BuildSuccessPage(), "text/html");
            }
            else
            {
                return Results.Content(OAuthResponseBuilder.BuildErrorPage(result.ErrorMessage!), "text/html");
            }
        });
    }
    
    
}