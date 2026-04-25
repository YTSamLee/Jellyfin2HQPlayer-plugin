using System;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Jellyfin2HQPlayer.EntryPoint
{
    public class PostScanRebuildTask : ILibraryPostScanTask
    {
        private readonly Services.PathIndexService _pathIndexService;
        private readonly ILogger<PostScanRebuildTask> _logger;

        public PostScanRebuildTask(
            Services.PathIndexService pathIndexService,
            ILogger<PostScanRebuildTask> logger)
        {
            _pathIndexService = pathIndexService;
            _logger = logger;
        }

        public string Name => "Jellyfin2HQPlayer Post-Scan Rebuild";

        public async Task Run(IProgress<double> progress, CancellationToken cancellationToken)
        {
            _logger.LogInformation("[J2H]: library scan completed, rebuilding path to id...");

            await _pathIndexService.RebuildAsync(cancellationToken).ConfigureAwait(false);

            progress.Report(100);

            _logger.LogInformation("[J2H]: path to id rebuild completed after library scan.");
        }
    }
}
