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