using System.Text;
using System.Text.Json;
using Shared.Contracts.DTOs.Auth;

namespace ApiGateway.Services;

public class AuthClient
{
    private readonly HttpClient _http;

    public AuthClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json"
        );

        var response = await _http.PostAsync("http://auth-service:5003/auth/login", content);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Auth error: {error}");
        }

        var json = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<AuthResponse>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }

    public async Task RegisterAsync(RegisterRequest request)
    {
        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json"
        );

        var response = await _http.PostAsync("http://auth-service:5003/auth/register", content);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            
            // Better error handling - preserve the actual error message
            if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            {
                // Try to extract the error message from the response
                if (string.IsNullOrEmpty(error))
                {
                    throw new Exception("User already exists or invalid data");
                }
            }
            
            throw new Exception(error);
        }
    }
}