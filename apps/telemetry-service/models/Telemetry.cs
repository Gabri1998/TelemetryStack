namespace TelemetryService.Models;

public class Telemetry
{
    public required Guid DeviceId { get; set; }

    public required double Temperature { get; set; }

    public required double Speed { get; set; }

    public required double Battery { get; set; }

    public required DateTime Timestamp { get; set; }
}