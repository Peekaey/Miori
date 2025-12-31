namespace Miori.Models.Osu;

public class OsuCommon
{
    
}

public class OsuCover
{
    public string custom_url { get; set; } 
    public string url { get; set; }
}

public class OsuScoreStatistics
{
    public int count_100 { get; set; }
    public int count_300 { get; set; }
    public int count_50 { get; set; }
    public int? count_geki { get; set; }
    public int? count_katu { get; set; }
    public int count_miss { get; set; }
}

public class OsuCurrentUserAttributes
{
    public OsuPin pin { get; set; }
}

public class OsuPin
{
    public bool is_pinned { get; set; }
    public long score_id { get; set; }
}

public class OsuBeatmap
{
    public int beatmapset_id { get; set; }
    public double difficulty_rating { get; set; }
    public int id { get; set; }
    public string mode { get; set; }
    public string status { get; set; }
    public int total_length { get; set; }
    public int user_id { get; set; }
    public string version { get; set; }
    public double accuracy { get; set; }
    public double ar { get; set; }
    public double bpm { get; set; }
    public bool convert { get; set; }
    public int count_circles { get; set; }
    public int count_sliders { get; set; }
    public int count_spinners { get; set; }
    public double cs { get; set; }
    public DateTime? deleted_at { get; set; }
    public double drain { get; set; }
    public int hit_length { get; set; }
    public bool is_scoreable { get; set; }
    public DateTime last_updated { get; set; }
    public int mode_int { get; set; }
    public int passcount { get; set; }
    public int playcount { get; set; }
    public int ranked { get; set; }
    public string url { get; set; }
    public string checksum { get; set; }
}

public class OsuBeatmapset
{
    public bool anime_cover { get; set; }
    public string artist { get; set; }
    public string artist_unicode { get; set; }
    public OsuCovers covers { get; set; }
    public string creator { get; set; }
    public int favourite_count { get; set; }
    public int genre_id { get; set; }
    public int? hype { get; set; }
    public int id { get; set; }
    public int language_id { get; set; }
    public bool nsfw { get; set; }
    public int offset { get; set; }
    public long play_count { get; set; }
    public string preview_url { get; set; }
    public string source { get; set; }
    public bool spotlight { get; set; }
    public string status { get; set; }
    public string title { get; set; }
    public string title_unicode { get; set; }
    public int? track_id { get; set; }
    public int user_id { get; set; }
    public bool video { get; set; }
}

public class OsuCovers
{
    public string cover { get; set; }
    public string cover2x { get; set; }
    public string card { get; set; }
    public string card2x { get; set; }
    public string list { get; set; }
    public string list2x { get; set; }
    public string slimcover { get; set; }
    public string slimcover2x { get; set; }
}

public class OsuUser
{
    public string avatar_url { get; set; }
    public string country_code { get; set; }
    public string default_group { get; set; }
    public long id { get; set; }
    public bool is_active { get; set; }
    public bool is_bot { get; set; }
    public bool is_deleted { get; set; }
    public bool is_online { get; set; }
    public bool is_supporter { get; set; }
    public DateTime last_visit { get; set; }
    public bool pm_friends_only { get; set; }
    public string profile_colour { get; set; }
    public string username { get; set; }
}