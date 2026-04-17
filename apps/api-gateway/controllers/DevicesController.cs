using Microsoft.AspNetCore.Mvc;
using ApiGateway.Models;
using ApiGateway.Services;
using Shared.Contracts.DTOs.Device;
namespace ApiGateway.Controllers;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/devices")]
[Authorize]
public class DevicesController : ControllerBase
{
    // Private field to store the injected service
private readonly DeviceClient _client;

private readonly TelemetryClient _telemetryClient;

public DevicesController(DeviceClient client, TelemetryClient telemetryClient)
{
    _client = client;
    _telemetryClient = telemetryClient;
}

    [HttpGet]
    public async Task<IActionResult> GetDevices()
    {
        // Call the service instead of creating data here
       var json = await _client.GetDevicesAsync();
         return Content(json, "application/json");
    }

[HttpGet("{deviceId}/telemetry")]
public async Task<IActionResult> GetTelemetry(string deviceId, [FromQuery] int limit = 50)
{   

    deviceId = deviceId.Trim();

    if (!Guid.TryParse(deviceId, out _))
        return BadRequest("Invalid deviceId");

    var data = await _telemetryClient.GetTelemetryAsync(deviceId, limit);
return Ok(data);
}


      [HttpPost]
public async Task<IActionResult> CreateDevice(CreateDeviceRequest request)
{
    if (string.IsNullOrWhiteSpace(request.Name))
        return BadRequest("Device name is required");

    await _client.CreateDeviceAsync(request.Name);
    return Ok();
}


[HttpGet("{deviceId}/status")]
public async Task<IActionResult> GetStatus(string deviceId)
{
    deviceId = deviceId.Trim();


    if (!Guid.TryParse(deviceId, out _))
        return BadRequest("Invalid deviceId");

    var status = await _telemetryClient.GetStatusAsync(deviceId);
return Ok(status);
}

}