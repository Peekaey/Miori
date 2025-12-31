namespace Miori.Models.Osu;

public class OsuMappedDto
{
    public int Id { get; set; }
    public string Avatar_url { get; set; }
    public string Cover_url { get; set; }
    public string Username { get; set; }
    public DateTimeOffset Join_date { get; set; }
    public OsuCoverDto Cover { get; set; }
    public List<OsuRecentScoreDto> RecentScores { get; set; } = new List<OsuRecentScoreDto>();
}

public class OsuCoverDto
{
    public string Custom_url { get; set; }
    public string Url { get; set; }
}

public class OsuRecentScoreDto
{
    public double Accuracy { get; set; }
    public long Id { get; set; }
    public int Max_combo { get; set; }
    public string Mode { get; set; }
    public List<string> Mods { get; set; }
    public bool Passed { get; set; }
    public double PP { get; set; }
    public string Rank { get; set; }
    public long Score { get; set; }
    public OsuScoreStatisticsDto Statistics { get; set; }
    public OsuBeatmapDto Beatmap { get; set; }
    public OsuBeatmapSetDto BeatmapSet { get; set; }
}

public class OsuBeatmapSetDto
{
    public string Artist { get; set; }
    public string Creator { get; set; }
    public int Id { get; set; }
    public string Title { get; set; }
    public string Status {get;set;}
    public string Preview_url { get; set; }
    public OsuBeatmapSetCoversDto Covers { get; set; }
}

public class OsuBeatmapSetCoversDto
{
    public string Cover { get; set; }
    public string Cover2x { get; set; }
    public string Card { get; set; }
    public string Card2x { get; set; }
    public string List { get; set; }
    public string List2x { get; set; }
    public string SlimCover { get; set; }
    public string SlimCover2x { get; set; }
}

public class OsuBeatmapDto
{
   public double Difficulty_rating { get; set; }
   public int Id { get; set; }
   public string Mode { get; set; }
   public int Ranked { get; set; }
   public string Version { get; set; }
   public double Accuracy { get; set; }
   public double Ar { get; set; }
   public double Bpm { get; set; }
   public double Drain { get; set; }
   public string Url { get; set; }
}

public class OsuScoreStatisticsDto
{
    public int Count_100 { get; set; }
    public int Count_300 { get; set; }
    public int Count_50 { get; set; }
    public int? Count_geki { get; set; }
    public int? Count_katu { get; set; }
    public int Count_miss { get; set; }
}

