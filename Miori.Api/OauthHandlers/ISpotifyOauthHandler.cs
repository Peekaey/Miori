using Miori.Models.Results;

namespace Miori.Api.OauthHandlers;

public interface ISpotifyOauthHandler
{
    Task<OAuthCallbackResult> HandleCallbackAsync(string? code, string? error);
}