namespace ShoukoV2.BackgroundService;

public interface IBackgroundWorkerService
{
    Task RefreshAllCachesAsync();
    Task<bool> RefreshSpotifyDataCache();
    Task<bool> RefreshAnilistDataCache();
}