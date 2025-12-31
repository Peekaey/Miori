namespace Miori.Models.Osu;

public class OsuMeResponse
{
    public int id { get; set; }
    public string avatar_url { get; set; }
    public string username { get; set; }
    public string cover_url { get; set; }
    public DateTimeOffset join_date { get; set; }
    public OsuCover cover { get; set; }
}