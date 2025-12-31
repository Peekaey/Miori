# Miori

Discord bot that collects rich presence data from discord as well as user data from other services such as Steam, Anilist, Spotify (More to come) & presents it via a REST api (SignalR websocket also available for presence data).

Information aggregated such as :
- Discord rich presence data (activities, state, asset links, etc)
- Spotify (basic user profile metadata, user playlists, user recently played)
- Anilist (basic user profile metadata, statistics of total anime/manga read/watched)
- Steam (basic user profile metadata, data/playtime of recently played games)

This information can then be retrived via a REST api endpoint (SignalR websocket is also available for rich presence).

The purpose of this bot is to act as an aggregation of data when attempting to display data in personal portfolio sites from various integrations, such as displaying current activity in discord or currently listening song on spotify.

This bot is currently considered feature complete for the time being. However more integrations or exposed data will be implemented adhoc as required (If you are after certain integrations or data, feel free to open an issue and I'll see if I can get it implemented).

### Additional Misc Features
-  Supports logging to Grafana Loki
-  Uses HybridCache for logging - Supports L1 in-memory cache and L2 distributed cache (Redis)
-  Spotify/Anilist authorisation over OAuth2 (auth link sent ephemerally via discord)
-  No persistent storage of data (access tokens are only stored in memory) - However requires reauthorising on every startup unless using L2 cache
-  Enable/Disable option of using API Keys to protect endpoints

Example Response:
```
{
  "DiscordUserData": {
    "uuid": 181661376584876032,
    "banner_url": "",
    "avatar_url": "https://cdn.discordapp.com/avatars/181661376584876032/9111cd4349bd70afdfeae68ab2e2936c.png",
    "global_username": "peekaey",
    "created_at": "2016-05-16T06:57:27.283+00:00",
    "activities": [
      {
        "name": "Custom Status",
        "state": "// 🔥 fire the event",
        "details": "",
        "large_text": "",
        "large_image": "",
        "small_image": "",
        "small_text": "",
        "timestamp_start_utc": "0001-01-01T00:00:00",
        "timestamp_end_utc": "0001-01-01T00:00:00",
        "activity_type": "Custom",
        "created_at_utc": "2025-11-12T23:47:09.916Z"
      },
      {
        "name": "Hang Status",
        "state": "chilling:illocons",
        "details": "",
        "large_text": "",
        "large_image": "",
        "small_image": "",
        "small_text": "",
        "timestamp_start_utc": "0001-01-01T00:00:00",
        "timestamp_end_utc": "0001-01-01T00:00:00",
        "activity_type": "6",
        "created_at_utc": "2025-11-13T03:44:34.101Z"
      },
      {
        "name": "Visual Studio Code",
        "state": "Workspace: albo",
        "details": "Editing swagger.json",
        "large_text": "Editing a JSON file",
        "large_image": "https://cdn.discordapp.com/app-assets/$383226320970055681/$1359299015484768338.png",
        "small_image": "https://cdn.discordapp.com/app-assets/$383226320970055681/$1359299466493956258.png",
        "small_text": "Visual Studio Code",
        "timestamp_start_utc": "2025-11-13T03:13:09.607",
        "timestamp_end_utc": "0001-01-01T00:00:00",
        "activity_type": "Playing",
        "created_at_utc": "2025-11-13T03:43:42.256Z"
      }
    ]
  },
  "AnilistUserData": {
    "id": 286644,
    "name": "Peekaey",
    "siteUrl": "https://anilist.co/user/",
    "avatar_url": "https://s4.anilist.co/file/anilistcdn/user/avatar/large/b286644-zuRiMWIANEdZ.png",
    "banner_url": "https://s4.anilist.co/file/anilistcdn/user/banner/b286644-ymkJ6JeoqqNI.jpg",
    "statistics": {
      "anime": {
        "count": 237,
        "mean_score": 79.37,
        "episodes_watched": 3636,
        "minutes_watched": 90679
      },
      "manga": {
        "chapters_read": 30134,
        "volumes_read": 1891,
        "count": 164,
        "mean_score": 81.22
      }
    }
  },
  "SpotifyUserData": {
    "display_name": "Peekaey",
    "profile_url": "https://open.spotify.com/user/503dof0042too2cldxtunfuk4",
    "avatar_url": "https://i.scdn.co/image/ab6775700000ee8508938050ec79453d7c47b31a",
    "recently_played": [
      {
        "track_name": "Luv(sic.) pt3 (feat. Shing02)",
        "track_id": "4xlpJ99yL9xYQtzG6c3hwk",
        "track_url": "https://open.spotify.com/track/4xlpJ99yL9xYQtzG6c3hwk",
        "played_at_utc": "2025-11-12T00:00:00+00:00",
        "artists": [
          {
            "artist_name": "Nujabes",
            "artist_id": "3Rq3YOF9YG9YfCWD4D56RZ",
            "artist_url": "https://open.spotify.com/artist/3Rq3YOF9YG9YfCWD4D56RZ"
          },
          {
            "artist_name": "Shing02",
            "artist_id": "0FB6beTn4vescDdnHeCUm9",
            "artist_url": "https://open.spotify.com/artist/0FB6beTn4vescDdnHeCUm9"
          }
        ],
        "combined_artists": "Nujabes, Shing02"
      }
    ],
    "user_playlists": [
      {
        "playlist_name": "El Jannah Crispy Box Runs",
        "playlist_id": "2LylLmSoEM4TyKPVwfDNqQ",
        "playlist_url": "https://open.spotify.com/playlist/2LylLmSoEM4TyKPVwfDNqQ",
        "playlist_cover_url": "https://image-cdn-fa.spotifycdn.com/image/ab67706c0000da847eede0de2b94ab003161678e",
        "playlist_description": "",
        "total_tracks": 18
      },
    ]
  },
  "SteamUserData": {
    "steamid": "76561198118248987",
    "profile_url": "https://steamcommunity.com/id/Peekaey/",
    "persona_name": "Peekaey",
    "avatar": "https://avatars.steamstatic.com/850572c9b37b12a501692806c858ac4cef6f58c6.jpg",
    "last_logoff_utc": "2025-11-12T21:33:32+00:00",
    "time_created_utc": "2013-12-15T04:02:55+00:00",
    "recent_games": [
      {
        "appid": 1422450,
        "name": "Deadlock",
        "playtime_2weeks_minutes": 1142,
        "playtime_forever_minutes": 8305,
        "img_icon_url": "https://cdn.cloudflare.steamstatic.com/steamcommunity/public/images/apps/1422450/f6da1420a173324d49bcd470fa3eee781ad0fa5e.jpg",
        "img_header_url": "https://cdn.cloudflare.steamstatic.com/steam/apps/1422450/header.jpg"
      }
    ]
  },
  "OsuUserData": {
    "id": 8611494,
    "avatar_url": "https://a.ppy.sh/8611494?1762034658.jpeg",
    "cover_url": "https://assets.ppy.sh/user-cover-presets/21/4e70615c027f579b2a887ec492857fb342fc0fc7f5586db62a9a67f115638c94.jpeg",
    "username": "Peekay",
    "join_date": "2016-06-25T11:46:52+00:00",
    "cover": {
      "custom_url": "https://assets.ppy.sh/user-cover-presets/21/4e70615c027f579b2a887ec492857fb342fc0fc7f5586db62a9a67f115638c94.jpeg",
      "url": "https://assets.ppy.sh/user-cover-presets/21/4e70615c027f579b2a887ec492857fb342fc0fc7f5586db62a9a67f115638c94.jpeg"
    },
    "recentScores": [
      {
        "accuracy": 0.9029745042492918,
        "id": 5986346143,
        "max_combo": 234,
        "mode": "osu",
        "mods": [],
        "passed": true,
        "pp": 49.6081,
        "rank": "B",
        "score": 0,
        "statistics": {
          "count_100": 77,
          "count_300": 611,
          "count_50": 5,
          "count_geki": null,
          "count_katu": null,
          "count_miss": 13
        },
        "beatmap": {
          "difficulty_rating": 4.86585,
          "id": 3862017,
          "mode": "osu",
          "ranked": 1,
          "version": "Kowari's Insane",
          "accuracy": 8,
          "ar": 9,
          "bpm": 174,
          "drain": 5,
          "url": "https://osu.ppy.sh/beatmaps/3862017"
        },
        "beatmapSet": {
          "artist": "Fox Stevenson",
          "creator": "Sotarks",
          "id": 1876102,
          "title": "Good Time",
          "status": "ranked",
          "preview_url": "//b.ppy.sh/preview/1876102.mp3",
          "covers": {
            "cover": "https://assets.ppy.sh/beatmaps/1876102/covers/cover.jpg?1668537488",
            "cover2x": null,
            "card": "https://assets.ppy.sh/beatmaps/1876102/covers/card.jpg?1668537488",
            "card2x": null,
            "list": "https://assets.ppy.sh/beatmaps/1876102/covers/list.jpg?1668537488",
            "list2x": null,
            "slimCover": "https://assets.ppy.sh/beatmaps/1876102/covers/slimcover.jpg?1668537488",
            "slimCover2x": null
          }
        }
      }
    ]
  }
}
```

Endpoints
```
REST
Discord:           /v1/user/{discordUserid}/discord
Spotify:           /v1/user/{discordUserid}/spotify
Anilist:           /v1/user/{discordUserid}/anilist
Osu:               /v1/user/{discordUserid}/osu
Steam:             /v1/user/{discordUserid}/steam/{steamUsername}
All(W/O Steam):    /v1/user/{discordUserid}/all
All(With Steam):   /v1/user/{discordUserid}/all?steamId={steamUsername}

Socket (SignalR)
Discord:           /socket/dp

PARAMS/HEADERS
REST AUTH (If Enabled): Header: x-api-key Value: apikey 
SIGNALR AUTH (If Enabled): Header: x-api-key Value: apikey
SIGNALR userId: Header: userId Value: discordUserId

Ideally would move this to swagger

```

## Docs - How to setup
1. Create a new discord bot app and ensure that all relevant presence/intents are enabled.
1. Create a new guild in discord to host the bot in (Any members in this guild will have their presence data available via the API)
1. Gather the parameters/config required as seen in the example.appsettings.json file
2. Start the application with ```dotnet build``` then ```dotnet run```
3. Authenticate with the Spotify/Anilist api by requesting the authorisation url through the bot commands (```/authenticate-with-spotify``` & ```/authenticate-with-anilist```) and completing the flow
4. After authentication, endpoints are ready to be used

## Docker
Can pull the latest image directly via ```ghcr.io/peekaey/miori:latest```, otherwise   
1. Build the docker image with ```docker build -t miori:latest .```
2. Start the container
<details>
  <summary>
    Container Start Command
  </summary>     
<pre lang="bash"><code>
docker run -d \
--name miori \
-p 0.0.0.0:5001:5001 \
-e DiscordBotToken="" \
-e DiscordOwnerGuildId="" \
-e SpotifyClientId="" \
-e SpotifyClientSecret="" \
-e SpotifyRedirectUri="" \
-e SpotifyScope="user-read-recently-played" \
-e AnilistClientId="" \
-e AnilistClientSecret="" \
-e AnilistRedirectUri="" \
-e EnableCaching="True" \
-e EnableRemoteLogging="True" \
-e RemoteCacheConnectionString="" \
-e LokiUsername="" \
-e LokiApiToken="" \
-e LokiUrl="" \
-e NetworkPort="5001" \
-e EnableApiKey="True" \
-e ApiKey="" \
-e StateSigningKey="" \
-e SteamApiKey="" \
-e OsuClientId="" \
-e OsuClientSecret="" \
-e OsuRedirectUri="" \
-e OsuScope="public"
miori:latest
</code>
</details>    

## FAQ
Q. Can I only use certain integrations instead of all 4?    
A. Yes! Providing the discord guildId/bot token is mandatory. However the current validation is that config values are required for every integration, although this can be disabled easily in the program startup.      
Q. Is there any setup required from Spotify/Anilist/Steam    
A. Yes, you will need to make a OAuth application for both [Spotify](https://developer.spotify.com/dashboard]) and [Anilist](https://anilist.co/settings/developer) and provide the clientid/client secret/redirect uri for each service to the bot. Steam just requires an api key which can be obtained following the documentation found at [Steam Web API Documentation](https://steamcommunity.com/dev)

### Future Roadmap
- Additional services (such as goodreads or apple music)
- More thorough data provided or flexible queries

### Credits    
- Heavily inspired by [Lanyard](https://github.com/Phineas/lanyard). If you are after a simple, easy to use API purely for discord presence data. I would highly recommend
