using Microsoft.AspNetCore.Mvc;
using Miori.Api;
using Miori.Api.OauthHandlers;
using Miori.BackgroundService;

namespace Miori.DiscordBot;

public static class OAuthEndpoints
{
    public static void MapOauthEndpoints(WebApplication host)
    {
        MapSpotifyOauthEndpoints(host);
        MapAnilistOauthEndpoints(host);
        MapOsuOauthEndpoints(host);
    }

    private static void MapSpotifyOauthEndpoints(WebApplication host)
    {
        host.MapGet("/callback/spotify", async (
            HttpContext context,
            [FromServices] ISpotifyOauthHandler oAuthHandler) =>
        {
            var code = context.Request.Query["code"].ToString();
            var error = context.Request.Query["error"].ToString();
            var state = context.Request.Query["state"].ToString();
            var result = await oAuthHandler.HandleCallbackAsync(code, error, state);
            if (result.IsSuccess)
            {
                return Results.Content(OAuthResponseBuilder.BuildSuccessPage(), "text/html");
            }
            else
            {
                return Results.Content(OAuthResponseBuilder.BuildErrorPage(result.ErrorMessage!), "text/html");
            }
        });
    }

    private static void MapAnilistOauthEndpoints(WebApplication host)
    {
        host.MapGet("/callback/anilist", async (
            HttpContext context,
            [FromServices] IAnilistOauthHandler oAuthHandler) =>
        {
            var code = context.Request.Query["code"].ToString();
            var error = context.Request.Query["error"].ToString();
            var state = context.Request.Query["state"].ToString();
            
            
            var result = await oAuthHandler.HandleCallbackAsync(code, error, state);
            if (result.IsSuccess)
            {
                return Results.Content(OAuthResponseBuilder.BuildSuccessPage(), "text/html");
            }
            else
            {
                return Results.Content(OAuthResponseBuilder.BuildErrorPage(result.ErrorMessage!), "text/html");
            }
        });
    }
    
    private static void MapOsuOauthEndpoints(WebApplication host)
    {
        host.MapGet("/callback/osu", async (
            HttpContext context,
            [FromServices] IOsuOauthHandler oAuthHandler) =>
        {
            var code = context.Request.Query["code"].ToString();
            var error = context.Request.Query["error"].ToString();
            var state = context.Request.Query["state"].ToString();
            
            
            var result = await oAuthHandler.HandleCallbackAsync(code, error, state);
            if (result.IsSuccess)
            {
                return Results.Content(OAuthResponseBuilder.BuildSuccessPage(), "text/html");
            }
            else
            {
                return Results.Content(OAuthResponseBuilder.BuildErrorPage(result.ErrorMessage!), "text/html");
            }
        });
    }
}