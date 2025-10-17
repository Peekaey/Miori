namespace ShoukoV2.Models.Anilist;

public class AnilistViewerResponse
{
    public AnilistViewerData data { get; set; } = new();
}

public class AnilistViewerData
{
    public AnilistViewer Viewer { get; set; } = new();
}

public class AnilistViewer
{
    public int id { get; set; }
    public string name { get; set; } = string.Empty;
    public AnilistAvatar avatar { get; set; } = new();
    public string? bannerImage { get; set; }
}

public class AnilistAvatar
{
    public string large { get; set; } = string.Empty;
}