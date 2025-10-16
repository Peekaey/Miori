namespace ShoukoV2.Models.Spotify;

public class SpotifyMeResponse
{
    public string display_name { get; set; } = string.Empty;
    public SpotifyExternalUrls external_urls { get; set; } = new();
    public SpotifyFollowers followers { get; set; } = new();
    public string href { get; set; } = string.Empty;
    public string id { get; set; } = string.Empty;
    public List<SpotifyImage> images { get; set; } = new();
    public string type { get; set; } = string.Empty;
    public string uri { get; set; } = string.Empty;
}

public class SpotifyExternalUrls
{
    public string spotify { get; set; } = string.Empty;
}

public class SpotifyFollowers
{
    public string? href { get; set; }
    public int total { get; set; }
}

public class SpotifyImage
{
    public int height { get; set; }
    public string url { get; set; } = string.Empty;
    public int width { get; set; }
}