using System;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.Jellyfin2HQPlayer.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Jellyfin2HQPlayer.EntryPoint;

public class StartupBuildService : IHostedService
{
    private readonly PathIndexService _pathIndexService;
    private readonly ILogger<StartupBuildService> _logger;

    public StartupBuildService(
        PathIndexService pathIndexService,
        ILogger<StartupBuildService> logger)
    {
        _pathIndexService = pathIndexService;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                _logger.LogInformation("[J2H]PathIndex: start building...");

                var start = DateTime.UtcNow;

                await _pathIndexService.RebuildAsync(cancellationToken);

                var elapsed = (DateTime.UtcNow - start).TotalSeconds;

                _logger.LogInformation("[J2H]PathIndex: done, count={Count}, time={Elapsed}s",
                    _pathIndexService.Count,
                    elapsed);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[J2H]PathIndex: build failed");
            }
        }, cancellationToken);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
