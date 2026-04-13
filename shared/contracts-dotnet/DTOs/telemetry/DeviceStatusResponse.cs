namespace Shared.Contracts.DTOs.Telemetry;

public class DeviceStatusResponse
{
    public string DeviceId { get; set; } = default!;
    public bool Online { get; set; }
}