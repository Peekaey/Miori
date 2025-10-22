namespace ShoukoV2.Models.Anilist;

public class AnilistProfileDto
{
    // TODO clean up the response object to only include necessary data
    public AnilistViewerStatisticsResponse AnilistViewerStatistics { get; set; } = new AnilistViewerStatisticsResponse();
}