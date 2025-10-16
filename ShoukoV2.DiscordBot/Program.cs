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
        
        var builder = Host.CreateApplicationBuilder(args);
        
        builder.Configuration
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", 
                optional: true, 
                reloadOnChange: true)
            .AddEnvironmentVariables();
        
        
        ValidateEnvironmentVariables(builder);
        
        ConfigureNetCordBuilder(builder);
        ConfigureServices(builder);
        var host = builder.Build();
        
        ConfigureNetcordHost(host);
        host.Run();
        Console.WriteLine("ShoukoV2 is running...");
    }

    static void ValidateEnvironmentVariables(HostApplicationBuilder builder)
    {
        var discordBotToken = builder.Configuration["DiscordBotToken"];
        if (string.IsNullOrEmpty(discordBotToken))
        {
            throw new ArgumentException("DiscordBotToken is required", nameof(discordBotToken));
        }
    }

    static void ConfigureNetCordBuilder(HostApplicationBuilder builder)
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

    static void ConfigureServices(HostApplicationBuilder builder)
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
        // Web Server for Spotify Oauth Callback Server
        builder.Services.AddHttpClient();
        builder.Services.AddSingleton<SpotifyOauthCallbackServer>();
        builder.Services.AddHostedService(sp => sp.GetRequiredService<SpotifyOauthCallbackServer>());
    }

    static void ConfigureNetcordHost(IHost host)
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
    
    
    private static void ConfigureDatabaseService(HostApplicationBuilder builder)
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
    
}