using Microsoft.AspNetCore.Mvc;

namespace Jellyfin.Plugin.Jellyfin2HQPlayer.Api;

[ApiController]
[Route("Plugins/Jellyfin2HQPlayer")]
public class PingController : ControllerBase
{
    [HttpGet("ping")]
    public ActionResult<string> Ping()
    {
        return "pong";
    }
}
