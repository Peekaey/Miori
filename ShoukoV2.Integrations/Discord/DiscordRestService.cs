using Microsoft.Extensions.Logging;
using NetCord;
using NetCord.Gateway;
using NetCord.Rest;
using ShoukoV2.DiscordBot.Internal.Interfaces;

namespace ShoukoV2.DiscordBot.Internal;

public class DiscordRestService : IDiscordRestService
{
    private readonly ILogger<DiscordRestService> _logger;
    private readonly RestClient _restClient;
    
    public DiscordRestService(RestClient restClient, ILogger<DiscordRestService> logger)
    {
        _restClient = restClient;
        _logger = logger;
    }

    public async Task<GuildUser?> GetGuildMemberAsync(ulong guildId, ulong uuid)
    {
        var member = await _restClient.GetGuildUserAsync(guildId,uuid);
        return member;
    }
    
    
}