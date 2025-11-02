namespace Miori.BackgroundService;

public interface IBackgroundWorkerService
{
    Task RefreshAllCachesAsync();
    Task RefreshSpotifyDataCache();
    Task RefreshAnilistDataCache();
}