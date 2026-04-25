using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.Jellyfin2HQPlayer.Services;
using Microsoft.AspNetCore.Mvc;

namespace Jellyfin.Plugin.Jellyfin2HQPlayer.Api
{
    [ApiController]
    [Route("Plugins/Jellyfin2HQPlayer")]
    public class PathIndexController : ControllerBase
    {
        private readonly PathIndexService _pathIndexService;

        public PathIndexController(PathIndexService pathIndexService)
        {
            _pathIndexService = pathIndexService;
        }

        // 路由：/Plugins/Jellyfin2HQPlayer/path2id?path={path}
        [HttpGet("path2id")]
        public IActionResult PathToId([FromQuery] string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return BadRequest(new
                {
                    ok = false,
                    message = "path is required"
                });
            }

            var id = _pathIndexService.Lookup(path);

            return Ok(new
            {
                ok = true,
                ready = _pathIndexService.Ready,
                path,
                normalizedPath = PathIndexService.NormalizePath(path),
                found = !string.IsNullOrWhiteSpace(id),
                id
            });
        }

        // 路由：/Plugins/Jellyfin2HQPlayer/rebuild
        [HttpPost("rebuild")]
        public async Task<IActionResult> Rebuild(CancellationToken cancellationToken)
        {
            await _pathIndexService.RebuildAsync(cancellationToken);

            return Ok(new
            {
                ok = true,
                ready = _pathIndexService.Ready,
                count = _pathIndexService.Count,
                totalAudioCount = _pathIndexService.TotalAudioCount,  // 确保返回音频文件总数
                lastUpdated = _pathIndexService.LastUpdated
            });
        }

        // 路由：/Plugins/Jellyfin2HQPlayer/status
        [HttpGet("status")]
        public IActionResult Status()
        {
            return Ok(new
            {
                ok = true,
                ready = _pathIndexService.Ready,
                count = _pathIndexService.Count,
                totalAudioCount = _pathIndexService.TotalAudioCount,  // 返回音频文件总数
                lastUpdated = _pathIndexService.LastUpdated
            });
        }
    }
}
