using Microsoft.AspNetCore.Mvc;
using ApiGateway.Models;
using ApiGateway.Services;
using ApiGateway.DTOs;
namespace ApiGateway.Controllers;

[ApiController]
[Route("api/devices")]
public class DevicesController : ControllerBase
{
    // Private field to store the injected service
private readonly DeviceClient _client;

public DevicesController(DeviceClient client)
{
    _client = client;
}

    [HttpGet]
    public async Task<IActionResult> GetDevices()
    {
        // Call the service instead of creating data here
       var json = await _client.GetDevicesAsync();
         return Content(json, "application/json");
    }

        [HttpPost]
    public async Task<IActionResult> CreateDevice([FromBody] CreateDeviceDto dto)
    {
        await _client.CreateDeviceAsync(dto.Name);
         return Ok();
    }
}