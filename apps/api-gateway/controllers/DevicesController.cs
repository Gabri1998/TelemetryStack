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
    private readonly DeviceService _deviceService;

    // Constructor (this is where DI happens)
    public DevicesController(DeviceService deviceService)
    {
        // .NET automatically injects DeviceService here
        _deviceService = deviceService;
    }

    [HttpGet]
    public IActionResult GetDevices()
    {
        // Call the service instead of creating data here
        var devices = _deviceService.GetDevices();

        return Ok(devices);
    }

        [HttpPost]
    public IActionResult CreateDevice([FromBody] CreateDeviceDto dto)
    {
        _deviceService.AddDevice(dto.Name);

        return Ok();
    }
}