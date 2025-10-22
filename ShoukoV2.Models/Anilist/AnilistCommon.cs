namespace ShoukoV2.Models.Anilist;

public class AnilistCommon
{
    
}

public class AnilistViewerStatisticsData
{
    public AnilistViewer Viewer { get; set; } = new();
}

public class AnilistViewer
{
    public int id { get; set; }
    public string name { get; set; }
    public string? bannerImage { get; set; }
    public AnilistUserStatistics statistics { get; set; } = new();
    public AnilistAvatar avatar { get; set; } = new();
}

public class AnilistAvatar
{
    public string large { get; set; } = string.Empty;
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

