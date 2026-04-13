using Shared.Contracts.DTOs.Device;
using DeviceService.Services;
using Microsoft.AspNetCore.Mvc;

namespace DeviceService.Controllers;

[ApiController]
[Route("api/devices")]
public class DevicesController : ControllerBase
{
    private readonly DeviceManager _service;

    public DevicesController(DeviceManager service)
    {
        _service = service;
    }

    [HttpGet]
    public IActionResult GetDevices()
    {
        return Ok(_service.GetDevices());
    }

    [HttpPost]
    public IActionResult CreateDevice([FromBody] CreateDeviceRequest request)
    {
        _service.AddDevice(request.Name);
        return Ok();
    }
}