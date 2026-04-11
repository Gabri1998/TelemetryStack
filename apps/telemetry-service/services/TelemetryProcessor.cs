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
        var deviceId = telemetry.DeviceId.ToString().ToLowerInvariant();

        //  Store last seen (used for "online" status)
        var lastSeenKey = $"device:lastSeen:{deviceId}";
        await _redisDb.StringSetAsync(
            lastSeenKey,
            DateTime.UtcNow.ToString("O")
        );

        // Push to Redis queue
        var json = JsonSerializer.Serialize(new QueueItem
        {
            Data = telemetry,
            RetryCount = 0
        });

        await _redisDb.ListRightPushAsync("telemetry_queue", json);

        // Push to SignalR clients
        await _hub.Clients.Group(deviceId)
            .SendAsync("ReceiveTelemetry", telemetry);

        Console.WriteLine($"Pushed to clients ({deviceId})");
    }
}