using DeviceService.DTOs;
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
    public IActionResult CreateDevice([FromBody] CreateDeviceDto dto)
    {
        _service.AddDevice(dto.Name);
        return Ok();
    }
}