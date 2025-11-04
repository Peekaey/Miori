using Microsoft.Extensions.Logging;
using Miori.BusinessService.Interfaces;
using Miori.Cache.Interfaces;
using Miori.Models;
using Miori.Models.Configuration;
using Miori.Models.Enums;
using Miori.Models.Steam;

namespace Miori.BusinessService;

public class SteamBusinessService : ISteamBusinessService
{
    private readonly ILogger<SteamBusinessService> _logger;
    private readonly ISteamCacheService  _steamCacheService;


    public SteamBusinessService(ILogger<SteamBusinessService> logger, ISteamCacheService steamCacheService)
    {
        _logger = logger;
        _steamCacheService = steamCacheService;
    }

    public async Task<ApiResult<SteamApiDto>> GetSteamDataForApi(ulong steamId)
    {
        var steamUserDataResult =  await _steamCacheService.GetCachedSteamData(steamId);

        if (steamUserDataResult.ResultOutcome != ResultEnum.Success)
        {
            return ApiResult<SteamApiDto>.AsInternalError();
        }
        return ApiResult<SteamApiDto>.AsSuccess(steamUserDataResult.Data);
    }
    
}