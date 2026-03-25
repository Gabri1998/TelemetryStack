using Microsoft.AspNetCore.Mvc;
using TelemetryService.Services;

namespace TelemetryService.Controllers;

[ApiController]
[Route("api/telemetry")]
public class TelemetryController : ControllerBase
{
    private readonly TelemetryQueryService _service;

    public TelemetryController(TelemetryQueryService service)
    {
        _service = service;
    }

  [HttpGet("{deviceId}")]
public async Task<IActionResult> GetTelemetry(string deviceId, [FromQuery] int limit = 50)
{
    if (!Guid.TryParse(deviceId, out var guid))
        return BadRequest("Invalid deviceId format");

    var data = await _service.GetLatestAsync(guid, limit);
    return Ok(data);
}


}