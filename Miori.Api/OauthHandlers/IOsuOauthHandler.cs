using Miori.Models.Results;

namespace Miori.Api.OauthHandlers;

public interface IOsuOauthHandler
{
    Task<OAuthCallbackResult> HandleCallbackAsync(string? code, string? error, string? state);
}