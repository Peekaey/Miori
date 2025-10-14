using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Services;
using NetCord.Hosting.Services.ApplicationCommands;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using ShoukoV2.DiscordBot.Internal;
using ShoukoV2.DiscordBot.Internal.Interfaces;
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
        builder.Services.AddSingleton<IGuildMemberHelpers, GuildMemberHelpers>();
        
    }

    static void ConfigureNetcordHost(IHost host)
    {
        host.AddModules(typeof(Program).Assembly);
    }
}