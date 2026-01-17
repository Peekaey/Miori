using Miori.Models.Anilist;
using Miori.Models.Discord;
using Miori.Models.Osu;
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
            ActivityType = activity.Type.ToString()
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

    public static AnilistMappedDto MapToApiDto(this AnilistResponseDto anilistResponseDto)
    {
        return new AnilistMappedDto
        {
            Id = anilistResponseDto.AnilistProfileResponse.data.viewer.id,
            Name = anilistResponseDto.AnilistProfileResponse.data.viewer.name,
            SiteUrl = anilistResponseDto.AnilistProfileResponse.data.viewer.siteUrl,
            AvatarUrl = anilistResponseDto.AnilistProfileResponse.data.viewer.avatar.large,
            BannerImageUrl = anilistResponseDto.AnilistProfileResponse.data.viewer.bannerImage ?? string.Empty,
            Statistics = new AnilistMappedStatisticsDto
            {
                Anime = new AnilistMappedStatisticsAnimeDto
                {
                    Count = anilistResponseDto.AnilistProfileResponse.data.viewer.statistics.anime.count,
                    MeanScore = anilistResponseDto.AnilistProfileResponse.data.viewer.statistics.anime.meanScore,
                    EpisodesWatched = anilistResponseDto.AnilistProfileResponse.data.viewer.statistics.anime.episodesWatched,
                    MinutesWatched = anilistResponseDto.AnilistProfileResponse.data.viewer.statistics.anime.minutesWatched
                },
                Manga = new AnilistMappedStatisticsMangaDto
                {
                    ChaptersRead = anilistResponseDto.AnilistProfileResponse.data.viewer.statistics.manga.chaptersRead,
                    Count = anilistResponseDto.AnilistProfileResponse.data.viewer.statistics.manga.count,
                    MeanScore = anilistResponseDto.AnilistProfileResponse.data.viewer.statistics.manga.meanScore,
                    VolumesRead = anilistResponseDto.AnilistProfileResponse.data.viewer.statistics.manga.volumesRead,
                }
            },
            Activities = anilistResponseDto.AnilistActivityResponse?.Data?.Page?.Activities?
                .Select(MapActivity)
                .ToList() ?? new List<AnilistMappedActivityDto>()
        };
    }

    private static AnilistMappedActivityDto MapActivity(ActivityBase activity)
    {
        return new AnilistMappedActivityDto
        {
            Id = activity.Id,
            Type = activity.Type,
            CreatedAt = activity.CreatedAt,
            
            Message = activity.Message,
            Messenger = activity.Messenger != null ? MapUser(activity.Messenger) : null,
            Recipient = activity.Recipient != null ? MapUser(activity.Recipient) : null,
            
            Status = activity.Status,
            Progress = activity.Progress,
            Media = activity.Media != null ? MapMedia(activity.Media) : null,
            
            Text = activity.Text,
            
            User = activity.User != null ? MapUser(activity.User) : null,
            Likes = activity.Likes?.Select(MapUser).ToList() ?? new List<AnilistMappedUserDto>(),
            Replies = activity.Replies?.Select(MapReply).ToList() ?? new List<AnilistMappedReplyDto>()
        };
    }

    private static AnilistMappedUserDto MapUser(User user)
    {
        return new AnilistMappedUserDto
        {
            Id = user.Id,
            Name = user.Name,
            AvatarUrl = user.Avatar?.Large ?? string.Empty
        };
    }

    private static AnilistMappedMediaDto MapMedia(Media media)
    {
        return new AnilistMappedMediaDto
        {
            Id = media.Id,
            Type = media.Type,
            TitleRomaji = media.Title?.Romaji ?? string.Empty,
            TitleEnglish = media.Title?.English ?? string.Empty,
            TitleNative = media.Title?.Native ?? string.Empty,
            CoverImageUrl = media.CoverImage?.Large ?? string.Empty
        };
    }

    private static AnilistMappedReplyDto MapReply(Reply reply)
    {
        return new AnilistMappedReplyDto
        {
            Id = reply.Id,
            Text = reply.Text,
            CreatedAt = reply.CreatedAt,
            User = reply.User != null ? MapUser(reply.User) : null
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
                PlayedAtUtc = track.played_at,
                Artists = track.track.artists.Select(artist => new SpotifyMappedArtistDto
                {
                    ArtistId = artist.id,
                    ArtistName = artist.name,
                    ArtistUrl = artist.external_urls.spotify
                }).ToList(),
                Album = new SpotifyMappedAlbumDto
                {
                    Name = track.track.album.name,
                    Covers = track.track.album.images.Select(cover => new SpotifyMappedAlbumCoverDto
                    {
                        url = cover.url
                    }).ToList()
                }
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
    
    public static OsuMappedDto MapToApiDto(this OsuProfileDto osuProfileDto)
    {
        return new OsuMappedDto
        {
            Id = osuProfileDto.OsuProfile.id,
            Avatar_url = osuProfileDto.OsuProfile.avatar_url,
            Cover_url = osuProfileDto.OsuProfile.cover_url,
            Username = osuProfileDto.OsuProfile.username,
            Join_date = osuProfileDto.OsuProfile.join_date,
            Cover = new OsuCoverDto
            {
                Custom_url = osuProfileDto.OsuProfile.cover.url,
                Url = osuProfileDto.OsuProfile.cover.url,
            },
            RecentScores = osuProfileDto.RecentScores.Select(score => new OsuRecentScoreDto
            {
                Accuracy = score.accuracy,
                Id = score.id,
                Max_combo = score.max_combo,
                Mode = score.mode,
                Mods = score.mods,
                Passed = score.passed,
                PP = score.pp,
                Rank = score.rank,
                Score = score.score,
                Statistics = new OsuScoreStatisticsDto
                {
                    Count_100 = score.statistics.count_100,
                    Count_300 = score.statistics.count_300,
                    Count_50 = score.statistics.count_50,
                    Count_geki = score.statistics.count_geki,
                    Count_katu = score.statistics.count_katu,
                    Count_miss = score.statistics.count_miss,
                },
                Beatmap = new OsuBeatmapDto
                {
                    Difficulty_rating = score.beatmap.difficulty_rating,
                    Id = score.beatmap.id,
                    Mode = score.beatmap.mode,
                    Ranked = score.beatmap.ranked,
                    Version = score.beatmap.version,
                    Accuracy = score.beatmap.accuracy,
                    Ar = score.beatmap.ar,
                    Bpm = score.beatmap.bpm,
                    Drain = score.beatmap.drain,
                    Url = score.beatmap.url,
                },
                BeatmapSet = new OsuBeatmapSetDto()
                {
                    Artist = score.beatmapset.artist,
                    Creator = score.beatmapset.creator,
                    Id = score.beatmapset.id,
                    Title = score.beatmapset.title,
                    Status = score.beatmapset.status,
                    Preview_url = score.beatmapset.preview_url,
                    Covers = new OsuBeatmapSetCoversDto
                    {
                        Cover = score.beatmapset.covers.cover,
                        Cover2x = score.beatmapset.covers.cover2x,
                        Card = score.beatmapset.covers.card,
                        Card2x = score.beatmapset.covers.card2x,
                        List = score.beatmapset.covers.list,
                        List2x = score.beatmapset.covers.list2x,
                        SlimCover = score.beatmapset.covers.slimcover,
                        SlimCover2x = score.beatmapset.covers.slimcover2x,
                    }
                }
            }).ToList()
        };
    }

    public static string ParseActvityImage(string? imageUrl, string? applicationId)
    {
        if (string.IsNullOrEmpty(imageUrl))
        {
            return string.Empty;
        }

        // Means its spotify album art
        // Remove the spotify: at the start and append the proper url
        if (imageUrl.Contains("spotify:"))
        {
            return imageUrl.Replace("spotify:", "https://i.scdn.co/image/");

        }
        
        if (imageUrl.Contains("mp:"))
        {
            return imageUrl.Replace("mp:", "https://media.discordapp.net/");
        }
        else
        {
            return $"https://cdn.discordapp.com/app-assets/{applicationId}/{imageUrl}.png";
        }
    }
    

}