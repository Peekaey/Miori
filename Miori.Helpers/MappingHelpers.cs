using Miori.Models.Anilist;
using Miori.Models.Discord;
using Miori.Models.Spotify;
using Miori.Models.Steam;
using NetCord;
using NetCord.Gateway;

namespace Miori.Helpers;

public static class MappingHelpers
{
    public static DiscordMappedDto MapToDto(this Presence presence)
    {
        return new DiscordMappedDto
        {
            Uuid = presence.User.Id,
            Activities = presence.Activities?.Select(activity => new DiscordActivityMappedDto
            {
                ActivityName = activity.Name,
                State = activity.State ?? string.Empty,
                Details = activity.Details ?? string.Empty,
                LargeText = activity.Assets?.LargeText ?? string.Empty,
                SmallText = activity.Assets?.SmallText ?? string.Empty,
                LargeImageId = ParseActvityImage(activity.Assets?.LargeImageId, activity.ApplicationId.ToString()),
                SmallImageId = ParseActvityImage(activity.Assets?.SmallImageId, activity.ApplicationId.ToString()),
                CreatedAtUtc = activity.CreatedAt.UtcDateTime,
                TimeStampStartUtc = activity.Timestamps?.StartTime?.DateTime ?? DateTime.MinValue,
                TimeStampEndUtc = activity.Timestamps?.EndTime?.DateTime ?? DateTime.MinValue,
                ActivityType = activity.Type.ToString(), 
            }).ToList() ?? new List<DiscordActivityMappedDto>()
        };
    }

    public static List<DiscordActivityMappedDto> MapActivitesToDto(this Presence presence)
    {
        return presence.Activities?.Select(activity => new DiscordActivityMappedDto
        {
            ActivityName = activity.Name,
            State = activity.State ?? string.Empty,
            Details = activity.Details ?? string.Empty,
            LargeText = activity.Assets?.LargeText ?? string.Empty,
            SmallText = activity.Assets?.SmallText ?? string.Empty,
            LargeImageId = ParseActvityImage(activity.Assets?.LargeImageId, activity.ApplicationId.ToString()),
            SmallImageId = ParseActvityImage(activity.Assets?.SmallImageId, activity.ApplicationId.ToString()),
            CreatedAtUtc = activity.CreatedAt.UtcDateTime,
            TimeStampStartUtc = activity.Timestamps?.StartTime?.DateTime ?? DateTime.MinValue,
            TimeStampEndUtc = activity.Timestamps?.EndTime?.DateTime ?? DateTime.MinValue,
            ActivityType = activity.Type.ToString(),
        }).ToList() ?? new List<DiscordActivityMappedDto>();
    }

    public static DiscordMappedDto MapUserDataToDto(this DiscordMappedDto dto, GuildUser user)
    {
        dto.AvatarUrl = user.GetAvatarUrl()?.ToString() ?? string.Empty;
        dto.BannerUrl = user.GetBannerUrl()?.ToString() ?? string.Empty;
        dto.CreatedAt = user.CreatedAt;
        dto.UserName = user.Username;
        dto.Uuid = user.Id;
        return dto;
    }

    public static SteamMappedDto MapToApiDto(this SteamApiResponses steamApiResponses)
    {   
        // Its ok if we use FirstOrDefault as we only allow inputs for one steamId at a time, so therefore steam API will
        // only return one response at a time
        return new SteamMappedDto
        {
            SteamId = steamApiResponses.PlayerSummaries.Response.Players.FirstOrDefault().SteamId ?? string.Empty,
            ProfileUrl = steamApiResponses.PlayerSummaries.Response.Players.FirstOrDefault()?.ProfileUrl ?? string.Empty,
            PersonaName = steamApiResponses.PlayerSummaries.Response.Players.FirstOrDefault()?.PersonaName ?? string.Empty,
            Avatar = steamApiResponses.PlayerSummaries.Response.Players.FirstOrDefault()?.Avatar ?? string.Empty,
            LastLogoffUtc = steamApiResponses.PlayerSummaries.Response.Players.FirstOrDefault()?.LastLogoff != null ? DateTimeOffset.FromUnixTimeSeconds(steamApiResponses.PlayerSummaries.Response.Players.FirstOrDefault().LastLogoff) : DateTime.MinValue,
            TimeCreatedUtc = steamApiResponses.PlayerSummaries.Response.Players.FirstOrDefault()?.TimeCreated != null ? DateTimeOffset.FromUnixTimeSeconds(steamApiResponses.PlayerSummaries.Response.Players.FirstOrDefault().TimeCreated) : DateTime.MinValue,
            RecentGames = steamApiResponses.RecentGames.Response.Games.Select(game => new SteamMappedRecentGamesDto
            {
                AppId = game.AppId,
                Name = game.Name,
                Playtime2Weeks = game.Playtime2Weeks,
                PlaytimeForever = game.PlaytimeForever,
                ImgIconUrl = game.ImgIconUrl != string.Empty ? $"https://cdn.cloudflare.steamstatic.com/steamcommunity/public/images/apps/{game.AppId}/{game.ImgIconUrl}.jpg" : string.Empty,
                ImgHeaderUrl = game.ImgIconUrl != string.Empty ? $"https://cdn.cloudflare.steamstatic.com/steam/apps/{game.AppId}/header.jpg" : string.Empty,
            }).ToList()
        };
    }

    public static AnilistMappedDto MapToApiDto(this AnilistProfileDto anilistProfileDto)
    {
        return new AnilistMappedDto
        {
            Id = anilistProfileDto.AnilistProfileResponse.data.viewer.id,
            Name = anilistProfileDto.AnilistProfileResponse.data.viewer.name,
            SiteUrl = anilistProfileDto.AnilistProfileResponse.data.viewer.siteUrl,
            AvatarUrl = anilistProfileDto.AnilistProfileResponse.data.viewer.avatar.large,
            BannerImageUrl = anilistProfileDto.AnilistProfileResponse.data.viewer.bannerImage ?? string.Empty,
            Statistics = new AnilistMappedStatisticsDto
            {
                Anime = new AnilistMappedStatisticsAnimeDto
                {
                    Count = anilistProfileDto.AnilistProfileResponse.data.viewer.statistics.anime.count,
                    MeanScore = anilistProfileDto.AnilistProfileResponse.data.viewer.statistics.anime.meanScore,
                    EpisodesWatched = anilistProfileDto.AnilistProfileResponse.data.viewer.statistics.anime.episodesWatched,
                    MinutesWatched = anilistProfileDto.AnilistProfileResponse.data.viewer.statistics.anime.minutesWatched
                },
                Manga = new AnilistMappedStatisticsMangaDto
                {
                    ChaptersRead = anilistProfileDto.AnilistProfileResponse.data.viewer.statistics.manga.chaptersRead,
                    Count = anilistProfileDto.AnilistProfileResponse.data.viewer.statistics.manga.count,
                    MeanScore = anilistProfileDto.AnilistProfileResponse.data.viewer.statistics.manga.meanScore,
                    VolumesRead = anilistProfileDto.AnilistProfileResponse.data.viewer.statistics.manga.volumesRead,
                }
            }
        };
    }

    public static SpotifyMappedDto MapToApiDto(this SpotifyProfileDto spotifyProfileDto)
    {
        return new SpotifyMappedDto
        {
            DisplayName = spotifyProfileDto.SpotifyProfile.display_name,
            AvatarUrl = spotifyProfileDto.SpotifyProfile.images.FirstOrDefault()?.url ?? string.Empty,
            ProfileUrl = spotifyProfileDto.SpotifyProfile.external_urls.spotify,
            RecentlyPlayed = spotifyProfileDto.RecentlyPlayed.items.Select(track => new SpotifyMappedRecentlyPlayedDto
            {
                TrackName = track.track.name,
                TrackId = track.track.id,
                TrackUrl = track.track.external_urls.spotify,
                PlayedAtUtc = track.played_at.Date,
                Artists = track.track.artists.Select(artist => new SpotifyMappedArtistDto
                {
                    ArtistId = artist.id,
                    ArtistName = artist.name,
                    ArtistUrl = artist.external_urls.spotify
                }).ToList()
            }).ToList(),
            UserPlaylists = spotifyProfileDto.UserPlaylists.items.Select(playlist => new SpotifyMappedUserPlaylistsResponse
            {
                PlaylistName = playlist.name,
                PlaylistId = playlist.id,
                PlaylistUrl = playlist.external_urls.spotify,
                PlaylistCoverUrl = playlist.images.FirstOrDefault()?.url ?? string.Empty,
                PlaylistDescription = playlist.description,
                TotalTracks = playlist.tracks.total
            }).ToList()
        };
    }

    public static string ParseActvityImage(string? imageUrl, string? applicationId)
    {
        if (string.IsNullOrEmpty(imageUrl) || string.IsNullOrEmpty(applicationId))
        {
            return string.Empty;
        }
        if (imageUrl.Contains("mp:"))
        {
            return imageUrl.Replace("mp:", "https://media.discordapp.net/");
        }
        else
        {
            return $"https://cdn.discordapp.com/app-assets/${applicationId}/${imageUrl}.png";
        }
    }
    

}