using System.Net.Http;

namespace ApiGateway.Services;

public class DeviceClient
{
    private readonly HttpClient _http;

    public DeviceClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<string> GetDevicesAsync()
    {
        return await _http.GetStringAsync("http://localhost:5001/api/devices");
    }

    public async Task CreateDeviceAsync(string name)
    {
        var content = new StringContent(
            System.Text.Json.JsonSerializer.Serialize(new { name }),
            System.Text.Encoding.UTF8,
            "application/json"
        );

        await _http.PostAsync("http://localhost:5001/api/devices", content);
    }
}