
namespace TelemetryService.DTOs;
public class TelemetryDto
{
    public string DeviceId { get; set; } = "";
    public double Temperature { get; set; }
    public double Speed { get; set; }
    public double Battery { get; set; }
    public string Timestamp { get; set; } = "";
}