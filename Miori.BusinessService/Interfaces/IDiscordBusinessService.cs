using Miori.Models;
using Miori.Models.Discord;
using NetCord;
using NetCord.Gateway;

namespace Miori.BusinessService.Interfaces;

public interface IDiscordBusinessService
{
    Task<Presence?> GetUserPresenceAsync(ulong guildId, ulong userId);
    Task<GuildUser?> GetGuildMemberAsync(ulong guildId, ulong uuid);
    Task<ApiResult<DiscordRichPresenceSocketDto>> GetDiscordPresence(ulong discordUserId);
}