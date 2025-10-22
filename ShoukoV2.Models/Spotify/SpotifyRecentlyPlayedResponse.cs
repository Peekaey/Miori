namespace ShoukoV2.Models.Spotify;

public class SpotifyRecentlyPlayedResponse
{
    public List<SpotifyPlayHistory> items { get; set; } = new();
    public string? next { get; set; }
    public SpotifyCursors cursors { get; set; } = new();
    public int limit { get; set; }
    public string href { get; set; } = string.Empty;
}
