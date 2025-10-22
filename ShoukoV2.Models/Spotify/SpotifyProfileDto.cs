namespace ShoukoV2.Models.Spotify;

public class SpotifyProfileDto
{
    // TODO clean up the response object to only include necessary data
    public SpotifyMeResponse SpotifyProfile { get; set; } = new SpotifyMeResponse();
    public SpotifyRecentlyPlayedResponse RecentlyPlayed { get; set; } = new SpotifyRecentlyPlayedResponse();
    public SpotifyUserPlaylistsResponse UserPlaylists { get; set; } = new SpotifyUserPlaylistsResponse();
}