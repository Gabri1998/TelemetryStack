using Microsoft.AspNetCore.SignalR;
using TelemetryService.Hubs;
using TelemetryService.Models;
using System.Text.Json;
using StackExchange.Redis;

public class TelemetryProcessor
{
    private readonly IDatabase _redisDb;
  private readonly IHubContext<TelemetryHub> _hub;

   public TelemetryProcessor(
    IConnectionMultiplexer redis,
    IHubContext<TelemetryHub> hub)
{
    _redisDb = redis.GetDatabase();
    _hub = hub;
}
    public async Task ProcessTelemetryAsync(Telemetry telemetry)
    {
        // 1. push to Redis queue
       var json = JsonSerializer.Serialize(new QueueItem
{
    Data = telemetry,
    RetryCount = 0
});

if (!Guid.TryParse(telemetry.DeviceId, out _))
{
    Console.WriteLine("Invalid telemetry, skipping SignalR");
    return;
}

var lastSeenKey = $"device:lastSeen:{telemetry.DeviceId.ToLowerInvariant()}";

await _redisDb.StringSetAsync(
    lastSeenKey,
    DateTime.UtcNow.ToString("O")
);

        await _redisDb.ListRightPushAsync("telemetry_queue", json);

   var group = telemetry.DeviceId.ToLowerInvariant();

await _hub.Clients.Group(group)
    .SendAsync("ReceiveTelemetry", telemetry);

        Console.WriteLine($"Pushed to clients ({telemetry.DeviceId})");
    }
}