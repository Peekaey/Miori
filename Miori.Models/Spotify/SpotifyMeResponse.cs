namespace Miori.Models.Spotify;

public class SpotifyMeResponse
{
    public string display_name { get; set; } = string.Empty;
    public SpotifyExternalUrls external_urls { get; set; } = new();
    // public SpotifyFollowers followers { get; set; } = new();
    // public string href { get; set; } = string.Empty;
    public string id { get; set; } = string.Empty;
    public List<SpotifyImage> images { get; set; } = new();
    // public string type { get; set; } = string.Empty;
    // public string uri { get; set; } = string.Empty;
}
