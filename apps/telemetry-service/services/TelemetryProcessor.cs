using System.Text.Json;
using StackExchange.Redis;
using TelemetryService.Models;

namespace TelemetryService.Services;

public class TelemetryProcessor
{
    private readonly IDatabase _redisDb;

    public TelemetryProcessor(IConnectionMultiplexer redis)
    {
        _redisDb = redis.GetDatabase();
    }

    public async Task ProcessTelemetryAsync(Telemetry telemetry)
    {
        //  Validation
        if (!Guid.TryParse(telemetry.DeviceId, out _))
        {
            Console.WriteLine(" Invalid DeviceId");
            return;
        }

        // Serialize to JSON
       var envelope = new QueueItem
        {
            Data = telemetry,
            RetryCount = 0
        };

        var json = JsonSerializer.Serialize(envelope);

        await _redisDb.ListRightPushAsync("telemetry_queue", json);

        Console.WriteLine(" Queued in Redis");
    }
}