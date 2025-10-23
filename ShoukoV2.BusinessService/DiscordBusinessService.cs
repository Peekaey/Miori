using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NetCord;
using NetCord.Gateway;
using ShoukoV2.BackgroundService;
using ShoukoV2.BusinessService.Interfaces;
using ShoukoV2.DiscordBot.Internal.Interfaces;
using ShoukoV2.Helpers.Discord;
using ShoukoV2.Models;
using ShoukoV2.Models.Discord;

namespace ShoukoV2.BusinessService;

public class DiscordBusinessService : IDiscordBusinessService
{
    private readonly ILogger<DiscordBusinessService> _logger;
    private readonly IDiscordRestService _discordRestService;
    private readonly IDiscordGatewayService  _discordGatewayService;
    private readonly IConfiguration _configuration;

    public DiscordBusinessService(ILogger<DiscordBusinessService> logger, IDiscordRestService discordRestService,
        IDiscordGatewayService discordGatewayService, IConfiguration configuration)
    {
        _logger = logger;
        _discordRestService = discordRestService;
        _discordGatewayService = discordGatewayService;
        _configuration = configuration;
    }

    // As the application is currently only intended get the data off a single guild/user
    // Hardcore the values for now
    // no need for caching as presence is realtime
    public async Task<Result<DiscordRichPresenceSocketDto>> GetDiscordPresence()
    {
        try
        {
            ulong guildId = ulong.Parse(_configuration["DiscordOwnerGuildId"]);
            ulong userId = ulong.Parse(_configuration["DiscordOwnerUserId"]);

            Presence? presence = await GetUserPresenceAsync(guildId, userId);

            if (presence == null)
            {
                return Result<DiscordRichPresenceSocketDto>.AsError("Unable to obtain presence of user");
            }
            
            return Result<DiscordRichPresenceSocketDto>.AsSuccess(presence.MapToDto());
        }
        catch (Exception ex)
        {
            return Result<DiscordRichPresenceSocketDto>.AsError(ex.Message);
        }
    }
    public async Task<Presence?> GetUserPresenceAsync(ulong guildId, ulong userId)
    {
        return await _discordGatewayService.GetUserPresenceAsync(guildId, userId);
    }

    public async Task<GuildUser?> GetGuildMemberAsync(ulong guildId, ulong uuid)
    {
        return await _discordRestService.GetGuildMemberAsync(guildId, uuid);
    }
}