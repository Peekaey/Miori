using Miori.Models.Results;

namespace Miori.Api.OauthHandlers;

public interface IAnilistOauthHandler
{
    Task<OAuthCallbackResult> HandleCallbackAsync(string? code, string? error);
}