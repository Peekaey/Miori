using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Miori.BusinessService.Interfaces;
using Miori.Helpers;
using Miori.Models;
using Miori.Models.Enums;

namespace Miori.BackgroundService;

public class BackgroundWorkerService : Microsoft.Extensions.Hosting.BackgroundService , IBackgroundWorkerService
{
    private readonly ILogger<BackgroundWorkerService> _logger;
    private readonly IServiceScopeFactory  _scopeFactory;
    private readonly TimeProvider _timeProvider;
    private readonly HybridCache _hybridCache;
    private readonly IConfiguration _configuration;
    
    public BackgroundWorkerService(ILogger<BackgroundWorkerService> logger, IServiceScopeFactory scopeFactory, TimeProvider timeProvider,
        HybridCache hybridCache, IConfiguration configuration)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _timeProvider = timeProvider;
        _hybridCache = hybridCache;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Background Service is starting...");
        using (var timer = new PeriodicTimer(TimeSpan.FromMinutes(30)))
        {
            try
            {
                while (await timer.WaitForNextTickAsync(stoppingToken))
                {

                }
            }
            catch (Exception ex)
            {
                _logger.LogApplicationException(DateTime.UtcNow, ex, "Exception from Background Worker Service");
                _logger.LogCritical("Background Service is stopping...");
            }
        }
    }
    
    
}