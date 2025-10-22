using NetCord;
using NetCord.Gateway;
using ShoukoV2.Models;
using ShoukoV2.Models.Discord;

namespace ShoukoV2.BusinessService.Interfaces;

public interface IDiscordBusinessService
{
    Task<Presence?> GetUserPresenceAsync(ulong guildId, ulong userId);
    Task<GuildUser?> GetGuildMemberAsync(ulong guildId, ulong uuid);
    Task<Result<DiscordPresenceDto>> GetDiscordPresence();
}