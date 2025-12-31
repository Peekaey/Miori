namespace Miori.Models.Osu;

public class OsuProfileDto
{
    public OsuMeResponse OsuProfile { get; set; } = new OsuMeResponse();
    public List<OsuRecentScoreResponse> RecentScores { get; set; } = new List<OsuRecentScoreResponse>();
}