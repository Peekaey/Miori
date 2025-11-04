# Miori

Discord bot that collects rich presence data from discord as well as user data from other services (Spotify & Anilist at this stage) & presents it via a REST api (Websocket also available for discord presence data).

Information aggregated such as :
- Discord rich presence data (activities, state, asset links, etc)
- Spotify (basic user profile metadata, user playlists, user recently played)
- Anilist (basic user profile metadata, statistics of total anime/manga read/watched)

This information can then be retrived via a rest api endpoint (SignalR websocket is also available for rich presence).

The purpose of this bot is to mostly serve as a utility and simplify the aggregation of data when attempting to display data in personal portfolio sites from various integrations. (Such as displaying current activity in discord or currently listening song on spotify on the site

### Additional Features
-  Supports logging to Grafana Loki
-  Supports HybridCache (defaults to refreshing & caching spotify/anilist/discord data every 30 minutes)
-  Spotify/Anilist authorisation over Oauth2
-  No persistent storage of data (access tokens are only stored in memory) - However requires reauthorising on every startup to obtain the tokens

<details>
  <summary>
    Endpoints
  </summary>       
    
       
  - api/v1/profile/spotify (REST)    
  - api/v1/profile/anilist (REST)    
  - api/v1/profile/discord (REST)    
  - /presencehub (Websocket)    
</details>

<details>
  <summary>
    Examples
    </summary>

  <details>
    <summary>
      Response format
    </summary>
        <pre lang="json"><code>
{
    "discordProfileData": {
        "uuid": "",
        "guildId": "",
        "discordActivities": [
            {
                "activityName": "",
                "state": "",
                "details": "",
                "largeText": "",
                "largeImageId": "",
                "smallImageId": "",
                "smallText": "",
                "timeStampStartUtc": "",
                "timeStampEndUtc": "",
                "activityType": "",
                "createdAtUtc": ""
            }
        ]
    },
    "anilistProfileData": {
        "anilistViewerStatistics": {
            "data": {
                "viewer": {
                    "id": 0,
                    "name": null,
                    "avatar": {
                        "large": ""
                    },
                    "bannerImage": null,
                    "statistics": {
                        "anime": {
                            "count": 0,
                            "meanScore": 0,
                            "volumesRead": 0
                        },
                        "manga": {
                            "chaptersRead": 0,
                            "count": 0,
                            "meanScore": 0
                        }
                    }
                }
            }
        }
    },
    "spotifyProfileData": {
        "spotifyProfile": {
            "display_name": "",
            "external_urls": {
                "spotify": ""
            },
            "id": "",
            "images": []
        },
        "recentlyPlayed": {
            "items": [],
            "next": null
        },
        "userPlaylists": {
            "total": 0,
            "items": []
        }
    }
}
</code></pre>
  </details>
</details>

## How to use
1. Complete parameters of appsettings.json (An example of the config/parameters required can be found in Miori.DiscordBot/appsettings.Development.json)
2. Start the application with ```dotnet build``` then ```dotnet run```
3. Authenticate with the Spotify/Anilist api by using the requesting the authorisation url through the discord bot commands
4. After authentication, endpoints are ready to be used

## Docker
1. Build the docker image with ```docker build -t miori:latest .```
2. Start the container with command:
<details>
  <summary>
    Container Start
  </summary>     
<pre lang="bash"><code>
docker run -d \
--name miori \
-p 0.0.0.0:5001:5001 \
-e DiscordBotToken="" \
-e DiscordOwnerUserId="" \
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
-e LokiUsername="" \
-e LokiApiToken="" \
-e LokiUrl="" \
-e NetworkPort="5001" \
miori:latest
</code>
</details>    

## FAQ
Q. Can I only use certain integrations instead of all 3?    
A. Yes! Only providing the discord owner/guildId is mandatory. However the current validation is that config values are required for every integration. However this can be disabled easily in the program startup.     
Q. How can I authenticate with the spotify/anilist api?    
A. By running the ```authenticate-with-spotify``` and ```authenticate-with-anilist``` bot commands which generate an Oauth authorisation URL to complete. This link is ephemeral and currently only executable by the user specified in the "DiscordOwnerUserId" parameter.    
Q. Is there any setup required from Spotify/Anilist    
A. Yes, you will need to make an OAuth application for each service respectively and pass through the displayed values when executing the program [Spotify](https://developer.spotify.com/dashboard]) [Anilist](https://anilist.co/settings/developer)    



### Credits    
- Inspired by [Lanyard](https://github.com/Phineas/lanyard)


### TO DO
- Fix up Api/SignalR response models
- Add automatic steamId lookup from custom vanity Url
- Look at logging/standardisation
- Decouple Spotify/Anilist/Steam data if the service isn't activated or the user isn't registered
- SignalR /all endpoint for aggregated data - webhooks for updates triggered by cache updates
- Elixir Api endpoint that can send a request through to it and it returns it back - api proxy endpoint / api gateway