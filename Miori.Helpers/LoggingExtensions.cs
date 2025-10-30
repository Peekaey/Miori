using Microsoft.Extensions.Logging;

namespace Miori.Helpers;

public static class LoggingExtensions
{
    public static void LogInteractionStart(this ILogger logger, string commandName, string userName, ulong userId, 
        string interactionId, DateTime interactionTimeUtc, ulong? guildId = null)
    {
        if (guildId.HasValue)
        {
            logger.LogInformation("{CommandName} command invoked by {UserName} ({UserId}) within guild {GuildId} " +
                                  "at {InteractionTimeUtc} with interactionId: {InteractionId}",
                commandName, userName, userId, guildId.Value, interactionTimeUtc, interactionId);
        }
        else
        {
            logger.LogInformation("{CommandName} command invoked by {UserName} ({UserId}) at {InteractionTimeUtc} with interactionId: {InteractionId}",
                commandName, userName, userId, interactionTimeUtc, interactionId);
        }
    }

    public static void LogInteractionEnd(this ILogger logger, string commandName, string userName, ulong userId,
        string interactionId, DateTime startTimeUtc, ulong? guildId = null)
    {
        DateTime endTimeUtc = DateTime.UtcNow;
        TimeSpan duration = endTimeUtc - startTimeUtc;

        if (guildId.HasValue)
        {
            logger.LogInformation("{CommandName} command completed successfully by {UserName} ({UserId}) within guild {GuildId} " +
                                  "at {InteractionTimeUtc} with interactionId: {InteractionId}. Duration: {DurationSeconds}s",
                commandName, userName, userId, guildId.Value, endTimeUtc, interactionId, duration.TotalSeconds);
        }
        else
        {
            logger.LogInformation("{CommandName} command completed successfully by {UserName} ({UserId}) at {InteractionTimeUtc} " +
                                  "with interactionId: {InteractionId}. Duration: {DurationSeconds}s",
                commandName, userName, userId, endTimeUtc, interactionId, duration.TotalSeconds);
        }
    }

    public static void LogInteractionException(this ILogger logger, Exception exception, string commandName, string userName, ulong userId,
        string interactionId, DateTime startTimeUtc, ulong? guildId = null)
    {
        DateTime endTimeUtc = DateTime.UtcNow;
        TimeSpan duration = endTimeUtc - startTimeUtc;

        if (guildId.HasValue)
        {
            logger.LogError(exception, "{CommandName} command completed with error by {UserName} ({UserId}) within guild {GuildId} " +
                                       "at {InteractionTimeUtc} with interactionId: {InteractionId}. Duration: {DurationSeconds}s",
                commandName, userName, userId, guildId.Value, endTimeUtc, interactionId, duration.TotalSeconds);
        }
        else
        {
            logger.LogError(exception, "{CommandName} command completed with error by {UserName} ({UserId}) at {InteractionTimeUtc} " +
                                       "with interactionId: {InteractionId}. Duration: {DurationSeconds}s",
                commandName, userName, userId, endTimeUtc, interactionId, duration.TotalSeconds);
        }
    }

    public static void LogApplicationError(this ILogger logger, DateTime timeUtc, string errorMessage)
    {
        logger.LogWarning("Application error occured with error: {errorMessage} at: {timeUtc} ", errorMessage , timeUtc);
    }

    public static void LogApplicationException(this ILogger logger, DateTime timeUtc, Exception exception, string? errorMessage = null)
    {
        logger.LogError(exception, "Application error occured at {timeUtc} with error: {errorMessage}", timeUtc, errorMessage);
    }

    public static void LogApplicationMessage(this ILogger logger, DateTime timeUtc, string message)
    {
        logger.LogInformation("{message} at: {timeUtc}", message, timeUtc);
    }

    public static void LogApiRequestStart(this ILogger logger, DateTime timeUtc, string requestId, string endpoint,
        string clientIp, string userAgent)
    {
        logger.LogInformation("Starting {endpoint} request at: {timeUtc}. RequestId: {requestId}, ClientIP: {clientIp}, UserAgent: {userAgent}",
            endpoint, timeUtc, requestId, clientIp, userAgent);
    }

    public static void LogApiRequestStartWithAuth(this ILogger logger, DateTime timeUtc, string requestId, 
        string endpoint, string clientIp, string userAgent, bool isAuthorized, string? authReason = null)
    {
        if (isAuthorized)
        {
            logger.LogInformation("Starting {endpoint} request at: {timeUtc}. RequestId: {requestId}, ClientIP: {clientIp}, UserAgent: {userAgent}, Auth: SUCCESS ({authReason})",
                endpoint, timeUtc, requestId, clientIp, userAgent, authReason);
        }
        else
        {
            logger.LogWarning("Starting {endpoint} request at: {timeUtc}. RequestId: {requestId}, ClientIP: {clientIp}, UserAgent: {userAgent}, Auth: FAILED ({authReason})",
                endpoint, timeUtc, requestId, clientIp, userAgent, authReason);
        }
    }
    
    public static void LogApiRequestEnd(this ILogger logger, DateTime timeUtc, string requestId, string endpoint,
        TimeSpan duration, int statusCode, Exception? exception = null)
    {
        if (exception != null)
        {
            logger.LogError(exception, "Completed {endpoint} request with error at: {timeUtc}. RequestId: {requestId}, Duration: {duration}ms , StatusCode: {statusCode} ",
                endpoint, timeUtc, requestId, duration.Milliseconds, statusCode);
        }
        else
        {
            logger.LogInformation("Completed {endpoint} request successfully at: {timeUtc}. RequestId: {requestId}, Duration: {duration}ms, StatusCode: {statusCode}", endpoint, timeUtc, requestId, duration.Milliseconds, statusCode);
        }
    }
    

}