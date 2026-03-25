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
        var url = $"http://localhost:5002/api/telemetry/{deviceId}?limit={limit}";
        return await _http.GetStringAsync(url);
    }
}