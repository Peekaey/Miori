using Miori.Models;

namespace Miori.BusinessService.Interfaces;

public interface IAggregateBusinessService
{
    Task<ApiResult<AggregateProfileDto>> GetAllProfileDataDto(ulong discordUserId, ulong? steamId);
}