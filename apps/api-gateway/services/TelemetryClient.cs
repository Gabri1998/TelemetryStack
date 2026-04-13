using System.Net.Http;
using System.Threading.Tasks;
using Shared.Contracts.DTOs.Telemetry;
using System.Text.Json;

namespace ApiGateway.Services;

public class TelemetryClient
{
    private readonly HttpClient _http;

    public TelemetryClient(HttpClient http)
    {
        _http = http;
    }
public async Task<List<TelemetryResponse>> GetTelemetryAsync(string deviceId, int limit = 50)
{
    var url = $"http://localhost:5001/api/telemetry/{deviceId}?limit={limit}";
    var response = await _http.GetAsync(url);

    if (!response.IsSuccessStatusCode)
        return new List<TelemetryResponse>();

    var json = await response.Content.ReadAsStringAsync();

    return JsonSerializer.Deserialize<List<TelemetryResponse>>(json,
        new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
        ?? new List<TelemetryResponse>();
}

public async Task<DeviceStatusResponse?> GetStatusAsync(string deviceId)
{
    var url = $"http://localhost:5001/api/telemetry/{deviceId}/status";
    var response = await _http.GetAsync(url);

    if (!response.IsSuccessStatusCode)
        return null;

    var json = await response.Content.ReadAsStringAsync();

    return JsonSerializer.Deserialize<DeviceStatusResponse>(json,
        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
}
}