using ApiGateway.Models;
using ApiGateway.Repositories;
using StackExchange.Redis;
using System.Text.Json;
 

namespace ApiGateway.Services;

public class DeviceService
{
    private readonly DeviceRepository _repository;
    private readonly IDatabase _cache;

    public DeviceService(DeviceRepository repository, IConnectionMultiplexer redis)
    {
        _repository = repository;

        // Get Redis database instance
        _cache = redis.GetDatabase();
    }

   public List<Device> GetDevices()
{
    var cacheKey = "devices:list";

    var cachedData = _cache.StringGet(cacheKey);

    Console.WriteLine($"Cache value BEFORE: {cachedData}");

    if (cachedData.HasValue && !string.IsNullOrWhiteSpace(cachedData))
    {
        Console.WriteLine("🔥 CACHE HIT");

        return JsonSerializer.Deserialize<List<Device>>(cachedData.ToString())
               ?? new List<Device>();
    }

    Console.WriteLine("💾 CACHE MISS → DB HIT");

    var devices = _repository.GetDevices();

    var json = JsonSerializer.Serialize(devices);

    Console.WriteLine("➡️ Writing to Redis...");

    var result = _cache.StringSet(cacheKey, json, TimeSpan.FromMinutes(5));

    Console.WriteLine($"✅ Redis write result: {result}");

    return devices;
}


        public void AddDevice(string name)
    {
        var device = new Device
        {
            Id = Guid.NewGuid().ToString(),
            Name = name
        };

        _repository.AddDevice(device);

        // ❗ Invalidate cache
        _cache.KeyDelete("devices:list");
    }
}