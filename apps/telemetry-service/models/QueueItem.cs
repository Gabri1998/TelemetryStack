namespace TelemetryService.Models;

public class QueueItem
{
    public Telemetry Data { get; set; } = default!;
    public int RetryCount { get; set; }
}