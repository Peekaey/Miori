using System.Text.Json.Serialization;

namespace Miori.Models.Anilist;

public class AnilistMappedDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("siteUrl")]
    public string SiteUrl { get; set; }
    [JsonPropertyName("avatar_url")]
    public string AvatarUrl { get; set; }
    [JsonPropertyName("banner_url")]
    public string BannerImageUrl { get; set; }
    [JsonPropertyName("statistics")]
    public AnilistMappedStatisticsDto Statistics { get; set; }
    [JsonPropertyName("activities")]
    public List<AnilistMappedActivityDto> Activities { get; set; }
}

public class AnilistMappedStatisticsDto
{
    [JsonPropertyName("anime")]
    public AnilistMappedStatisticsAnimeDto Anime { get; set; }
    [JsonPropertyName("manga")]
    public AnilistMappedStatisticsMangaDto Manga { get; set; }
}

public class AnilistMappedStatisticsAnimeDto
{
    [JsonPropertyName("count")]
    public int Count { get; set; }
    [JsonPropertyName("mean_score")]
    public double MeanScore { get; set; }
    [JsonPropertyName("episodes_watched")]
    public int EpisodesWatched { get; set; }
    [JsonPropertyName("minutes_watched")]
    public int MinutesWatched{ get; set; }
}

public class AnilistMappedStatisticsMangaDto
{
    [JsonPropertyName("chapters_read")]
    public int ChaptersRead { get; set; }
    [JsonPropertyName("volumes_read")]
    public int VolumesRead { get; set; }
    [JsonPropertyName("count")]
    public int Count { get; set; }
    [JsonPropertyName("mean_score")]
    public double MeanScore { get; set; }
}

public class AnilistMappedActivityDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("type")]
    public string Type { get; set; }
    [JsonPropertyName("created_at")]
    public int CreatedAt { get; set; }
    [JsonPropertyName("message")]
    public string Message { get; set; }
    [JsonPropertyName("messenger")]
    public AnilistMappedUserDto Messenger { get; set; }
    [JsonPropertyName("recipient")]
    public AnilistMappedUserDto Recipient { get; set; }
    [JsonPropertyName("status")]
    public string Status { get; set; }
    [JsonPropertyName("progress")]
    public string Progress { get; set; }
    [JsonPropertyName("media")]
    public AnilistMappedMediaDto Media { get; set; }
    [JsonPropertyName("text")]
    public string Text { get; set; }
    [JsonPropertyName("user")]
    public AnilistMappedUserDto User { get; set; }
    [JsonPropertyName("likes")]
    public List<AnilistMappedUserDto> Likes { get; set; }
    [JsonPropertyName("replies")]
    public List<AnilistMappedReplyDto> Replies { get; set; }
}

public class AnilistMappedUserDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("avatar_url")]
    public string AvatarUrl { get; set; }
}

public class AnilistMappedMediaDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("type")]
    public string Type { get; set; }
    [JsonPropertyName("title_romaji")]
    public string TitleRomaji { get; set; }
    [JsonPropertyName("title_english")]
    public string TitleEnglish { get; set; }
    [JsonPropertyName("title_native")]
    public string TitleNative { get; set; }
    [JsonPropertyName("cover_image_url")]
    public string CoverImageUrl { get; set; }
}

public class AnilistMappedReplyDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("text")]
    public string Text { get; set; }
    [JsonPropertyName("created_at")]
    public int CreatedAt { get; set; }
    [JsonPropertyName("user")]
    public AnilistMappedUserDto User { get; set; }
}