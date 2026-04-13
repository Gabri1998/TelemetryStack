// DTOs/Telemetry/TelemetryResponse.cs
namespace Shared.Contracts.DTOs.Telemetry;

public class TelemetryResponse
{
    public Guid DeviceId { get; set; }
    public double Temperature { get; set; }
    public double Speed { get; set; }
    public double Battery { get; set; }
    public DateTime Timestamp { get; set; }
}