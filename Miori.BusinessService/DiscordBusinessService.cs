using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Miori.BusinessService.Interfaces;
using Miori.Helpers;
using Miori.Integrations.Discord;
using Miori.Models;
using Miori.Models.Discord;
using NetCord;
using NetCord.Gateway;

namespace Miori.BusinessService;

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


    public async Task<Result<DiscordRichPresenceSocketDto>> GetDiscordPresence(ulong discordUserId)
    {
        try
        {
            ulong guildId = ulong.Parse(_configuration["DiscordOwnerGuildId"]);

            Presence? presence = await GetUserPresenceAsync(guildId, discordUserId);

            if (presence == null)
            {
                _logger.LogApplicationError(DateTime.UtcNow, $"Failed to get user presence for user {discordUserId}");
                return Result<DiscordRichPresenceSocketDto>.AsError($"Failed to get user presence {discordUserId}");
            }
            
            return Result<DiscordRichPresenceSocketDto>.AsSuccess(presence.MapToDto());
        }
        catch (Exception ex)
        {
            _logger.LogApplicationException(DateTime.UtcNow, ex, $"Error fetching presence of discord user {discordUserId}");
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