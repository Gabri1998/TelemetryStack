using TelemetryService.Models;
using TelemetryService.Repositories;
using StackExchange.Redis;
using System.Text.Json;


namespace TelemetryService.Services;

public class TelemetryQueryService
{
    
private readonly TelemetryRepository _repository;
private readonly IDatabase _cache;

public TelemetryQueryService(
    TelemetryRepository repository,
    IConnectionMultiplexer redis)
{
    _repository = repository;
    _cache = redis.GetDatabase();
}

public async Task<List<Telemetry>> GetLatestAsync(Guid deviceId, int limit = 50)
{
    var cacheKey = $"telemetry:{deviceId}";

    var cached = await _cache.StringGetAsync(cacheKey);

    List<Telemetry>? data;

    if (cached.HasValue)
    {
        Console.WriteLine("CACHE HIT");

        data = JsonSerializer.Deserialize<List<Telemetry>>(cached.ToString()) 
           ?? new List<Telemetry>();
    }
    else
    {
        Console.WriteLine("CACHE MISS");

        // Always fetch last 100 (fixed window)
        data = await _repository.GetLatestByDeviceAsync(deviceId, 100);

        var json = JsonSerializer.Serialize(data);

        await _cache.StringSetAsync(cacheKey, json, TimeSpan.FromSeconds(30));
    }

    return data?
    .OrderByDescending(x => x.Timestamp)
    .Take(limit)
    .ToList()
    ?? new List<Telemetry>();
}
}