using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ShoukoV2.Models.Spotify;

namespace ShoukoV2.Api.Anilist;

public interface IAnilistOauthHandler
{
    Task<OAuthCallbackResult> HandleCallbackAsync(string? code, string? error);
}