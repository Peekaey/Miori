using Microsoft.Extensions.Logging;
using ShoukoV2.BusinessService.Interfaces;
using ShoukoV2.Integrations.Anilist.Interfaces;
using ShoukoV2.Models;
using ShoukoV2.Models.Anilist;
using ShoukoV2.Models.Enums;

namespace ShoukoV2.BusinessService;

public class AnilistBusinessService : IAnilistBusinessService
{
    private readonly ILogger<AnilistBusinessService> _logger;
    private readonly IAnilistApiService  _anilistApiService;

    public AnilistBusinessService(ILogger<AnilistBusinessService> logger, IAnilistApiService anilistApiService)
    {
        _logger = logger;
        _anilistApiService = anilistApiService;
    }

    public async Task<Result<AnilistProfileDto>> GetAnilistProfile()
    {
        try
        {
            AnilistProfileDto anilistProfileDto = new AnilistProfileDto();
            var anilistProfileResult = await _anilistApiService.GetAnilistProfileStatistics();

            if (anilistProfileResult.ResultOutcome == ResultEnum.Success)
            {
                anilistProfileDto.AnilistViewerStatistics = anilistProfileResult.Data;
            }

            return Result<AnilistProfileDto>.AsSuccess(anilistProfileDto);
        }
        catch (Exception ex)
        {
            return Result<AnilistProfileDto>.AsError(ex.Message);
        }
    }
    
    
}