using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ShoukoV2.Helpers;

namespace ShoukoV2.Api.Rest;

public class RequestLoggingFilter : ActionFilterAttribute
{
    private const string StartTimeKey = "StartTime";
    private const string RequestIdKey = "RequestId";
    private const string AuthResultKey = "AuthResult";
    private const string AuthReasonKey = "AuthReason";
    
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var startTime = DateTime.UtcNow;
        var requestId = context.HttpContext.TraceIdentifier;
        var endpoint = $"{context.HttpContext.Request.Method}:{context.HttpContext.Request.Path}";
        var clientIp = GetClientIpAddress(context.HttpContext);
        var userAgent = context.HttpContext.Request.Headers.UserAgent.ToString();
        
        // Default to true if no auth filter
        var isAuthorised = context.HttpContext.Items[AuthResultKey] as bool? ?? true;
        var authReason = context.HttpContext.Items[AuthReasonKey] as string;
        
        context.HttpContext.Items[StartTimeKey] = startTime;
        context.HttpContext.Items[RequestIdKey] = requestId;
        
        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<RequestLoggingFilter>>();
        
        logger.LogApiRequestStartWithAuth(startTime, requestId, endpoint, clientIp, userAgent, isAuthorised, authReason);
        
    }

    public override void OnActionExecuted(ActionExecutedContext context)
    {
        var startTime = (DateTime)context.HttpContext.Items[StartTimeKey]!;
        var requestId = (string)context.HttpContext.Items[RequestIdKey]!;
        var duration = DateTime.UtcNow - startTime;
        var endpoint = $"{context.HttpContext.Request.Method}:{context.HttpContext.Request.Path}";
        var statusCode = (int)context.HttpContext.Response.StatusCode;
        
        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<RequestLoggingFilter>>();

        logger.LogApiRequestEnd(DateTime.UtcNow, requestId, endpoint, duration, statusCode, context.Exception);
    }

    // Optimise for cloudflare due to current setup
    private static string GetClientIpAddress(HttpContext context)
    {
        return context.Request.Headers["cf-connecting-ip"].FirstOrDefault() ??
               context.Request.Headers["X-Forwarded-For"].FirstOrDefault()?.Split(',')[0].Trim() ??
               context.Request.Headers["X-Real-IP"].FirstOrDefault() ??
               context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }
    
}