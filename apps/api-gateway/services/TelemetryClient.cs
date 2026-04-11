using System.Net.Http;
using System.Threading.Tasks;

namespace ApiGateway.Services;

public class TelemetryClient
{
    private readonly HttpClient _http;

    public TelemetryClient(HttpClient http)
    {
        _http = http;
    }

   public async Task<string> GetTelemetryAsync(string deviceId, int limit = 50)
{
    var url = $"http://localhost:5001/api/telemetry/{deviceId}?limit={limit}";

    var response = await _http.GetAsync(url);

    if (!response.IsSuccessStatusCode)
    {
        Console.WriteLine($"Telemetry fetch failed: {response.StatusCode}");
        return "[]"; // prevent crash
    }

    return await response.Content.ReadAsStringAsync();
}

public async Task<string> GetStatusAsync(string deviceId)
{
    var url = $"http://localhost:5001/api/telemetry/{deviceId}/status";

    var response = await _http.GetAsync(url);

    if (!response.IsSuccessStatusCode)
    {
        Console.WriteLine($"Status fetch failed: {response.StatusCode}");
        return "{\"online\": false}";
    }

    return await response.Content.ReadAsStringAsync();
}

}