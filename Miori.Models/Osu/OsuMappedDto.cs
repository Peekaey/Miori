using System.Text.Json.Serialization;

namespace Miori.Models.Osu;

public class OsuMappedDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("avatar_url")]
    public string Avatar_url { get; set; }
    [JsonPropertyName("cover_url")]
    public string Cover_url { get; set; }
    [JsonPropertyName("username")]
    public string Username { get; set; }
    [JsonPropertyName("url")]
    public string Url { get; set; }
    [JsonPropertyName("join_date")]
    public DateTimeOffset Join_date { get; set; }
    [JsonPropertyName("cover")]
    public OsuCoverDto Cover { get; set; }
    [JsonPropertyName("recent_scores")]
    public List<OsuRecentScoreDto> RecentScores { get; set; } = new List<OsuRecentScoreDto>();
}

public class OsuCoverDto
{
    [JsonPropertyName("custom_url")]
    public string Custom_url { get; set; }
    [JsonPropertyName("url")]
    public string Url { get; set; }
}

public class OsuRecentScoreDto
{
    [JsonPropertyName("accuracy")]
    public double Accuracy { get; set; }
    [JsonPropertyName("id")]
    public long Id { get; set; }
    [JsonPropertyName("max_combo")]
    public int Max_combo { get; set; }
    [JsonPropertyName("mode")]
    public string Mode { get; set; }
    [JsonPropertyName("mods")]
    public List<string> Mods { get; set; }
    [JsonPropertyName("passed")]
    public bool Passed { get; set; }
    [JsonPropertyName("pp")]
    public double PP { get; set; }
    [JsonPropertyName("rank")]
    public string Rank { get; set; }
    [JsonPropertyName("score")]
    public long Score { get; set; }
    [JsonPropertyName("statistics")]
    public OsuScoreStatisticsDto Statistics { get; set; }
    [JsonPropertyName("beatmap")]
    public OsuBeatmapDto Beatmap { get; set; }
    [JsonPropertyName("beatmapset")]
    public OsuBeatmapSetDto BeatmapSet { get; set; }
}

public class OsuBeatmapSetDto
{
    [JsonPropertyName("cover")]
    public OsuCoverDto Cover { get; set; }
    [JsonPropertyName("artist")]
    public string Artist { get; set; }
    [JsonPropertyName("creator")]
    public string Creator { get; set; }
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("title")]
    public string Title { get; set; }
    [JsonPropertyName("status")]
    public string Status {get;set;}
    [JsonPropertyName("preview_url")]
    public string Preview_url { get; set; }
    [JsonPropertyName("covers")]
    public OsuBeatmapSetCoversDto Covers { get; set; }
}

public class OsuBeatmapSetCoversDto
{
    [JsonPropertyName("covers")]
    public string Cover { get; set; }
    [JsonPropertyName("cover_2x")]
    public string Cover2x { get; set; }
    [JsonPropertyName("card")]
    public string Card { get; set; }
    [JsonPropertyName(("card_2x"))]
    public string Card2x { get; set; }
    [JsonPropertyName("list")]
    public string List { get; set; }
    [JsonPropertyName("list_2x")]
    public string List2x { get; set; }
    [JsonPropertyName("slim_cover")]
    public string SlimCover { get; set; }
    [JsonPropertyName("slim_cover_2x")]
    public string SlimCover2x { get; set; }
}

public class OsuBeatmapDto
{
    [JsonPropertyName("difficulty_rating")]
   public double Difficulty_rating { get; set; }
   [JsonPropertyName("id")]
   public int Id { get; set; }
   [JsonPropertyName("mode")]
   public string Mode { get; set; }
   [JsonPropertyName("ranked")]
   public int Ranked { get; set; }
   [JsonPropertyName("version")]
   public string Version { get; set; }
   [JsonPropertyName("accuracy")]
   public double Accuracy { get; set; }
   [JsonPropertyName("ar")]
   public double Ar { get; set; }
   [JsonPropertyName("bpm")]
   public double Bpm { get; set; }
   [JsonPropertyName("drain")]
   public double Drain { get; set; }
   [JsonPropertyName("url")]
   public string Url { get; set; }
}

public class OsuScoreStatisticsDto
{
    [JsonPropertyName("count_100")]
    public int Count_100 { get; set; }
    [JsonPropertyName("count_300")]
    public int Count_300 { get; set; }
    [JsonPropertyName("count_50")]
    public int Count_50 { get; set; }
    [JsonPropertyName("count_geki")]
    public int? Count_geki { get; set; }
    [JsonPropertyName("count_katu")]
    public int? Count_katu { get; set; }
    [JsonPropertyName("count_miss")]
    public int Count_miss { get; set; }
}

