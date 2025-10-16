namespace ShoukoV2.Models.Spotify;

public class SpotifyRecentlyPlayedResponse
{
    public List<SpotifyPlayHistory> items { get; set; } = new();
    public string? next { get; set; }
    public SpotifyCursors cursors { get; set; } = new();
    public int limit { get; set; }
    public string href { get; set; } = string.Empty;
}

public class SpotifyPlayHistory
{
    public SpotifyTrack track { get; set; } = new();
    public DateTime played_at { get; set; }
    public SpotifyContext? context { get; set; }
}

public class SpotifyTrack
{
    public SpotifyAlbum album { get; set; } = new();
    public List<SpotifyArtist> artists { get; set; } = new();
    public List<string> available_markets { get; set; } = new();
    public int disc_number { get; set; }
    public int duration_ms { get; set; }
    public bool @explicit { get; set; }
    public SpotifyExternalIds external_ids { get; set; } = new();
    public SpotifyExternalUrls external_urls { get; set; } = new();
    public string href { get; set; } = string.Empty;
    public string id { get; set; } = string.Empty;
    public bool is_local { get; set; }
    public string name { get; set; } = string.Empty;
    public int popularity { get; set; }
    public string? preview_url { get; set; }
    public int track_number { get; set; }
    public string type { get; set; } = string.Empty;
    public string uri { get; set; } = string.Empty;
}

public class SpotifyAlbum
{
    public string album_type { get; set; } = string.Empty;
    public List<SpotifyArtist> artists { get; set; } = new();
    public List<string> available_markets { get; set; } = new();
    public SpotifyExternalUrls external_urls { get; set; } = new();
    public string href { get; set; } = string.Empty;
    public string id { get; set; } = string.Empty;
    public List<SpotifyImage> images { get; set; } = new();
    public string name { get; set; } = string.Empty;
    public string release_date { get; set; } = string.Empty;
    public string release_date_precision { get; set; } = string.Empty;
    public int total_tracks { get; set; }
    public string type { get; set; } = string.Empty;
    public string uri { get; set; } = string.Empty;
}

public class SpotifyArtist
{
    public SpotifyExternalUrls external_urls { get; set; } = new();
    public string href { get; set; } = string.Empty;
    public string id { get; set; } = string.Empty;
    public string name { get; set; } = string.Empty;
    public string type { get; set; } = string.Empty;
    public string uri { get; set; } = string.Empty;
}

public class SpotifyExternalIds
{
    public string isrc { get; set; } = string.Empty;
}

public class SpotifyCursors
{
    public string after { get; set; } = string.Empty;
    public string before { get; set; } = string.Empty;
}

public class SpotifyContext
{
    public string? type { get; set; }
    public string? href { get; set; }
    public SpotifyExternalUrls? external_urls { get; set; }
    public string? uri { get; set; }
}