namespace Miori.Models.Spotify;

public class SpotifyCommon
{
    
}

public class SpotifyPlayHistory
{
    public SpotifyTrack track { get; set; } = new();
    public DateTime played_at { get; set; }
    // public SpotifyContext? context { get; set; }
}

public class SpotifyTrack
{
    public SpotifyAlbum album { get; set; } = new();
    public List<SpotifyArtist> artists { get; set; } = new();
    // public List<string> available_markets { get; set; } = new();
    // public int disc_number { get; set; }
    // public int duration_ms { get; set; }
    // public bool @explicit { get; set; }
    // public SpotifyExternalIds external_ids { get; set; } = new();
    public SpotifyExternalUrls external_urls { get; set; } = new();
    // public string href { get; set; } = string.Empty;
    public string id { get; set; } = string.Empty;
    // public bool is_local { get; set; }
    public string name { get; set; } = string.Empty;
    // public int popularity { get; set; }
    // public string? preview_url { get; set; }
    // public int track_number { get; set; }
    // public string type { get; set; } = string.Empty;
    // public string uri { get; set; } = string.Empty;
}

public class SpotifyAlbum
{
    // public string album_type { get; set; } = string.Empty;
    public List<SpotifyArtist> artists { get; set; } = new();
    // public List<string> available_markets { get; set; } = new();
    public SpotifyExternalUrls external_urls { get; set; } = new();
    // public string href { get; set; } = string.Empty;
    public string id { get; set; } = string.Empty;
    public List<SpotifyImage> images { get; set; } = new();
    public string name { get; set; } = string.Empty;
    // public string release_date { get; set; } = string.Empty;
    // public string release_date_precision { get; set; } = string.Empty;
    // public int total_tracks { get; set; }
    public string type { get; set; } = string.Empty;
    public string uri { get; set; } = string.Empty;
}

public class SpotifyArtist
{
    public SpotifyExternalUrls external_urls { get; set; } = new();
    // public string href { get; set; } = string.Empty;
    public string id { get; set; } = string.Empty;
    public string name { get; set; } = string.Empty;
    // public string type { get; set; } = string.Empty;
    // public string uri { get; set; } = string.Empty;
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

public class SpotifyPlaylist
{
    // public bool collaborative { get; set; }
    public string description { get; set; } = string.Empty;
    public SpotifyExternalUrls external_urls { get; set; } = new();
    // public string href { get; set; } = string.Empty;
    public string id { get; set; } = string.Empty;
    public List<SpotifyImage> images { get; set; } = new();
    public string name { get; set; } = string.Empty;
    public SpotifyPlaylistOwner owner { get; set; } = new();
    // public string? primary_color { get; set; }
    public bool @public { get; set; }
    // public string snapshot_id { get; set; } = string.Empty;
    public SpotifyPlaylistTracks tracks { get; set; } = new();
    // public string type { get; set; } = string.Empty;
    // public string uri { get; set; } = string.Empty;
}

public class SpotifyPlaylistOwner
{
    public string display_name { get; set; } = string.Empty;
    public SpotifyExternalUrls external_urls { get; set; } = new();
    // public string href { get; set; } = string.Empty;
    public string id { get; set; } = string.Empty;
    // public string type { get; set; } = string.Empty;
    // public string uri { get; set; } = string.Empty;
}

public class SpotifyPlaylistTracks
{
    // public string href { get; set; } = string.Empty;
    public int total { get; set; }
}

public class SpotifyImage
{
    public int? height { get; set; }
    public int? width { get; set; }
    public string url { get; set; } = string.Empty;
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