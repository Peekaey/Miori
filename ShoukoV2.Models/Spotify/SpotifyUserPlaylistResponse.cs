namespace ShoukoV2.Models.Spotify;

public class SpotifyUserPlaylistsResponse
{
    public string href { get; set; } = string.Empty;
    public int limit { get; set; }
    public string? next { get; set; }
    public int offset { get; set; }
    public string? previous { get; set; }
    public int total { get; set; }
    public List<SpotifyPlaylist> items { get; set; } = new();
}

public class SpotifyPlaylist
{
    public bool collaborative { get; set; }
    public string description { get; set; } = string.Empty;
    public SpotifyExternalUrls external_urls { get; set; } = new();
    public string href { get; set; } = string.Empty;
    public string id { get; set; } = string.Empty;
    public List<SpotifyImage> images { get; set; } = new();
    public string name { get; set; } = string.Empty;
    public SpotifyPlaylistOwner owner { get; set; } = new();
    public string? primary_color { get; set; }
    public bool @public { get; set; }
    public string snapshot_id { get; set; } = string.Empty;
    public SpotifyPlaylistTracks tracks { get; set; } = new();
    public string type { get; set; } = string.Empty;
    public string uri { get; set; } = string.Empty;
}

public class SpotifyPlaylistOwner
{
    public string display_name { get; set; } = string.Empty;
    public SpotifyExternalUrls external_urls { get; set; } = new();
    public string href { get; set; } = string.Empty;
    public string id { get; set; } = string.Empty;
    public string type { get; set; } = string.Empty;
    public string uri { get; set; } = string.Empty;
}

public class SpotifyPlaylistTracks
{
    public string href { get; set; } = string.Empty;
    public int total { get; set; }
}