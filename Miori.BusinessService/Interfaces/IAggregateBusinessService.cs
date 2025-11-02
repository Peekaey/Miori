using Miori.Models;

namespace Miori.BusinessService.Interfaces;

public interface IAggregateBusinessService
{
    Task<Result<AggregateProfileDto>> GetAllProfileDataDto(ulong discordUserId);
}