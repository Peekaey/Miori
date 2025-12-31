namespace Miori.Models.Osu;

public class OsuRecentScoreResponse
{
    public double accuracy { get; set; }
    public int? best_id { get; set; }
    public DateTime created_at { get; set; }
    public long id { get; set; }
    public int max_combo { get; set; }
    public string mode { get; set; }
    public int mode_int { get; set; }
    public List<string> mods { get; set; }
    public bool passed { get; set; }
    public bool perfect { get; set; }
    public double pp { get; set; }
    public string rank { get; set; }
    public bool replay { get; set; }
    public long score { get; set; }
    public OsuScoreStatistics statistics { get; set; }
    public string type { get; set; }
    public long user_id { get; set; }
    public OsuCurrentUserAttributes current_user_attributes { get; set; }
    public OsuBeatmap beatmap { get; set; }
    public OsuBeatmapset beatmapset { get; set; }
    public OsuUser user { get; set; }
}