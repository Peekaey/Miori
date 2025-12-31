
namespace Miori.DiscordBot;

class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Starting Miori...");
        
        var builder = WebApplication.CreateBuilder(args);
        Configuration.ValidateEnvironmentVariables(builder);
        Configuration.ConfigureAppSettings(builder);
        Configuration.ConfigureNetCordBuilder(builder);
        Configuration.ConfigureServices(builder);
        Configuration.AddOauthHandlerEndpoints(builder);
        Configuration.ConfigureRemoteLogging(builder);

        builder.Services.AddSwaggerGen();
        
        var host = builder.Build();
        Configuration.ConfigureHost(host);
        Configuration.ConfigureMinimalApiEndpoints(host);
        OAuthEndpoints.MapOauthEndpoints(host);

        if (!host.Environment.IsDevelopment())
        {
            host.UseSwagger();
            host.UseSwaggerUI();
        }

        host.Run();
        Console.WriteLine("Miori is running...");
    }
}