namespace Miori.Models.Results;

public class OAuthCallbackResult
{
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }

    public static OAuthCallbackResult Success() => new() { IsSuccess = true };
    public static OAuthCallbackResult Error(string message) => new() { IsSuccess = false, ErrorMessage = message };
}