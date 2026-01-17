namespace Miori.Models.Anilist;

public class AnilistResponseDto
{
    // TODO clean up the response object to only include necessary data
    public AnilistProfileResponse AnilistProfileResponse { get; set; }
    public AniListActivityResponse AnilistActivityResponse { get; set; }
}