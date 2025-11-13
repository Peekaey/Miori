namespace Miori.Models.Anilist;

public class AnilistCommon
{
    
}

public class AnilistData
{
    public AnilistViewer viewer { get; set; } = new();
}

public class AnilistViewerStatisticsData
{
    public AnilistViewer viewer { get; set; } = new();
}

public class AnilistViewer
{
    public int id { get; set; }
    public string name { get; set; }
    public string siteUrl { get; set; }
    public AnilistAvatar avatar { get; set; } = new();
    public string? bannerImage { get; set; }
    public AnilistUserStatistics statistics { get; set; } = new();
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
    public int episodesWatched { get; set; }
    public int minutesWatched { get; set; }
    public int count { get; set; }
    public double meanScore { get; set; }
}

public class AnilistMangaStatistics
{
    public int chaptersRead { get; set; }
    public int count { get; set; }
    public double meanScore { get; set; }
    public int volumesRead { get; set; }
}

