using Microsoft.Extensions.Logging;
using NetCord;
using NetCord.Rest;
using ShoukoV2.DiscordBot.Internal.Interfaces;

namespace ShoukoV2.DiscordBot.Internal;

public class GuildMemberHelpers : IGuildMemberHelpers
{
    private readonly ILogger<GuildMemberHelpers> _logger;
    private readonly RestClient _restClient;
    
    public GuildMemberHelpers(RestClient restClient, ILogger<GuildMemberHelpers> logger)
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