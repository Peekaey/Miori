using System.Text.Json.Serialization;

namespace Miori.Models.Spotify;

public class SpotifyMappedDto
{
    [JsonPropertyName("display_name")]
    public string DisplayName{ get; set; }
    [JsonPropertyName("profile_url")]
    public string ProfileUrl { get; set; }
    [JsonPropertyName("avatar_url")]
    public string AvatarUrl { get; set; }
    [JsonPropertyName("recently_played")]
    public List<SpotifyMappedRecentlyPlayedDto> RecentlyPlayed { get; set; }
    [JsonPropertyName("user_playlists")]
    public List<SpotifyMappedUserPlaylistsResponse> UserPlaylists { get; set; }
}

public class SpotifyMappedRecentlyPlayedDto
{
    [JsonPropertyName("track_name")]
    public string TrackName { get; set; }
    [JsonPropertyName("track_id")]
    public string TrackId { get; set; }
    [JsonPropertyName("track_url")]
    public string TrackUrl { get; set; }
    [JsonPropertyName("played_at_utc")]
    public DateTimeOffset PlayedAtUtc { get; set; }
    [JsonPropertyName("artists")]
    public List<SpotifyMappedArtistDto> Artists { get; set; }

    [JsonPropertyName("combined_artists")]
    public string CombinedArtists
    {
        get { return string.Join(", ", Artists.Select(artists => artists.ArtistName)); }
    }
    // TODO Future
    // [JsonPropertyName("track_cover_url")]
    // public string SongArtUrl { get; set; }
}

public class SpotifyMappedArtistDto
{
    [JsonPropertyName("artist_name")]
    public string ArtistName { get; set; }
    [JsonPropertyName("artist_id")]
    public string ArtistId { get; set; }
    [JsonPropertyName("artist_url")]
    public string ArtistUrl { get; set; }
}

public class SpotifyMappedUserPlaylistsResponse
{
    [JsonPropertyName("playlist_name")]
    public string PlaylistName { get; set; }
    [JsonPropertyName("playlist_id")]
    public string PlaylistId { get; set; }
    [JsonPropertyName("playlist_url")]
    public string PlaylistUrl { get; set; }
    [JsonPropertyName("playlist_cover_url")]
    public string PlaylistCoverUrl { get; set; }
    [JsonPropertyName("playlist_description")]
    public string PlaylistDescription { get; set; }
    [JsonPropertyName("total_tracks")]
    public int TotalTracks { get; set; }
    
}