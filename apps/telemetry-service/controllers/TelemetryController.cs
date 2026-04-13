using Microsoft.AspNetCore.Mvc;
using TelemetryService.Services;
using TelemetryService.Models;
using Shared.Contracts.DTOs.Telemetry;
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

var result = data.Select(t => new TelemetryResponse
{
    DeviceId = t.DeviceId,
    Temperature = t.Temperature,
    Speed = t.Speed,
    Battery = t.Battery,
    Timestamp = t.Timestamp
});

return Ok(result);
}

[HttpGet("{deviceId}/status")]
public async Task<IActionResult> GetStatus(string deviceId)
{
    var isOnline = await _statusService.IsOnlineAsync(deviceId);

   return Ok(new DeviceStatusResponse
{
    DeviceId = deviceId,
    Online = isOnline
});
}




}