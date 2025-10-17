namespace ShoukoV2.Models.Anilist;

public class AnilistViewerStatisticsResponse
{
    public AnilistViewerStatisticsData data { get; set; } = new();
}

public class AnilistViewerStatisticsData
{
    public AnilistViewerWithStatistics Viewer { get; set; } = new();
}

public class AnilistViewerWithStatistics
{
    public AnilistUserStatistics statistics { get; set; } = new();
}

public class AnilistUserStatistics
{
    public AnilistAnimeStatistics anime { get; set; } = new();
    public AnilistMangaStatistics manga { get; set; } = new();
}

public class AnilistAnimeStatistics
{
    public int count { get; set; }
    public double meanScore { get; set; }
    public int volumesRead { get; set; }
}

public class AnilistMangaStatistics
{
    public int chaptersRead { get; set; }
    public int count { get; set; }
    public double meanScore { get; set; }
}