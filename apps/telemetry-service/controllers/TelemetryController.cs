using Microsoft.AspNetCore.Mvc;
using TelemetryService.Services;
using TelemetryService.Models;
using StackExchange.Redis;
using System.Text.Json;

namespace TelemetryService.Controllers;

[ApiController]
[Route("api/telemetry")]
public class TelemetryController : ControllerBase
{
    private readonly TelemetryQueryService _service;
private readonly DeviceStatusService _statusService;

private readonly IDatabase _redisDb;

private readonly TelemetryProcessor _processor;
public TelemetryController(
    TelemetryQueryService service,
    DeviceStatusService statusService,
    IConnectionMultiplexer redis,
    TelemetryProcessor processor) 
{
    _service = service;
    _statusService = statusService;
    _redisDb = redis.GetDatabase();
      _processor = processor;   
}

  [HttpGet("{deviceId}")]
public async Task<IActionResult> GetTelemetry(string deviceId, [FromQuery] int limit = 50)
{
    if (!Guid.TryParse(deviceId, out var guid))
        return BadRequest("Invalid deviceId format");

    var data = await _service.GetLatestAsync(guid, limit);
    return Ok(data);
}

[HttpGet("{deviceId}/status")]
public async Task<IActionResult> GetStatus(string deviceId)
{
    var isOnline = await _statusService.IsOnlineAsync(deviceId);

    return Ok(new
    {
        deviceId,
        online = isOnline
    });
}


[HttpPost]
public async Task<IActionResult> PostTelemetry([FromBody] Telemetry telemetry)
{
    if (telemetry == null || !Guid.TryParse(telemetry.DeviceId, out _))
        return BadRequest("Invalid telemetry");

    await _processor.ProcessTelemetryAsync(telemetry);

    return Ok();
}

}