namespace Miori.Models.Spotify;


public class SpotifyUserPlaylistsResponse
{
    // public string href { get; set; } = string.Empty;
    // public int limit { get; set; }
    // public string? next { get; set; }
    // public int offset { get; set; }
    // public string? previous { get; set; }
    public int total { get; set; }
    public List<SpotifyPlaylist> items { get; set; } = new();
}
