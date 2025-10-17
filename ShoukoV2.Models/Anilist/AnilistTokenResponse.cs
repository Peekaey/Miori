namespace ShoukoV2.Models.Anilist;

public class AnilistTokenResponse
{
    public string token_type { get; set; } = string.Empty;
    public int expires_in { get; set; }
    public string access_token { get; set; } = string.Empty;
    public string refresh_token { get; set; } = string.Empty;
}