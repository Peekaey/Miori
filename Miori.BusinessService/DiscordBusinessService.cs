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


    public async Task<ApiResult<DiscordMappedDto>> GetDiscordPresence(ulong discordUserId)
    {
        try
        {
            ulong guildId = ulong.Parse(_configuration["DiscordOwnerGuildId"]);

            var presenceTask = GetUserPresenceAsync(guildId, discordUserId);
            var userTask = GetUserAsync(guildId, discordUserId);
            
            await Task.WhenAll(presenceTask, userTask);
            
            var presenceData =  await presenceTask;
            var userData = await userTask;
            
            DiscordMappedDto dto = new DiscordMappedDto();

            if (presenceData == null && userData == null)
            {
                _logger.LogApplicationError(DateTime.UtcNow, $"Failed to get user presence for user {discordUserId}");
                return ApiResult<DiscordMappedDto>.AsInternalError();
            }
            if (presenceData != null)
            {
                dto.Activities = presenceData.MapActivitesToDto();

            }

            if (userData != null)
            {
                dto.MapUserDataToDto(userData);
            }
            
            
            return ApiResult<DiscordMappedDto>.AsSuccess(dto);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationException(DateTime.UtcNow, ex, $"Error fetching presence of discord user {discordUserId}");
            return ApiResult<DiscordMappedDto>.AsInternalError();
        }
    }
    public async Task<Presence?> GetUserPresenceAsync(ulong guildId, ulong userId)
    {
        return await _discordGatewayService.GetUserPresenceAsync(guildId, userId);
    }

    public async Task<GuildUser?> GetUserAsync(ulong guildId, ulong userId)
    {
        return await _discordGatewayService.GetUserDataAsync(guildId, userId);
    }

    public async Task<GuildUser?> GetGuildMemberAsync(ulong guildId, ulong uuid)
    {
        return await _discordRestService.GetGuildMemberAsync(guildId, uuid);
    }
}