using ShoukoV2.Models;

namespace ShoukoV2.BusinessService.Interfaces;

public interface IAggregateBusinessService
{
    Task<Result<AggregateProfileDto>> GetAllProfileDataDto();
}