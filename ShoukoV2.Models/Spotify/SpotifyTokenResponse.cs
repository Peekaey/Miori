namespace ShoukoV2.Models.Spotify;

public class SpotifyTokenResponse
{
    public string Access_Token { get; set; } = string.Empty;
    public string Token_Type { get; set; } = string.Empty;
    public int Expires_In { get; set; }
    public string Refresh_Token { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;
}