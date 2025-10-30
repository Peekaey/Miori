using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Miori.Api.Rest;

public class ApiKeyAuthAttribute : Attribute, IAuthorizationFilter
{
    
    private const string _apiKeyHeaderName = "X-API-KEY";
    
    private const string AuthResultKey = "AuthResult";
    private const string AuthReasonKey = "AuthReason";
    
    public ApiKeyAuthAttribute()
    {

    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();

        if (configuration.GetValue<bool>("EnableApiKey") == false)
        {
            // Api keys are disabled
            context.HttpContext.Items[AuthResultKey] = true;
            context.HttpContext.Items[AuthReasonKey] = "API key validation disabled";
            return;
        }
        
            
        if (!context.HttpContext.Request.Headers.TryGetValue(_apiKeyHeaderName, out var apiKey))
        {
            context.HttpContext.Items[AuthResultKey] = false;
            context.HttpContext.Items[AuthReasonKey] = "API key missing";
            context.Result = new UnauthorizedObjectResult("Api Key Missing");
        }  
            
        var validApiKey = configuration.GetValue<string>("ApiKey");

        if (!validApiKey.Equals(apiKey))
        {
            context.HttpContext.Items[AuthResultKey] = false;
            context.HttpContext.Items[AuthReasonKey] = "Invalid API key";
            context.Result = new UnauthorizedObjectResult("Invalid Api Key");
        }
        
        context.HttpContext.Items[AuthResultKey] = true;
        context.HttpContext.Items[AuthReasonKey] = "API key validated successfully";
    }
    
    
}