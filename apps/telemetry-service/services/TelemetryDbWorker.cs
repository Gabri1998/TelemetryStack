using System.Text.Json;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;
using TelemetryService.Models;
using TelemetryService.Repositories;

namespace TelemetryService.Services;

public class TelemetryDbWorker : BackgroundService
{
    private readonly IDatabase _redisDb;
    private readonly TelemetryRepository _repository;

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public TelemetryDbWorker(
        IConnectionMultiplexer redis,
        TelemetryRepository repository)
    {
        _redisDb = redis.GetDatabase();
        _repository = repository;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    Console.WriteLine("DB Worker started");

    int failureDelayMs = 1000;
   

    while (!stoppingToken.IsCancellationRequested)
    {
        // If DB is down → pause consumption entirely
        

        var value = await _redisDb.ListLeftPopAsync("telemetry_queue");

        if (value.IsNullOrEmpty)
        {
            await Task.Delay(500, stoppingToken);
            continue;
        }

        try
{
    var json = value!.ToString();

    var envelope = JsonSerializer.Deserialize<QueueItem>(json, _jsonOptions);

    if (envelope == null)
    {
        Console.WriteLine("Invalid envelope → DLQ");

        await _redisDb.ListRightPushAsync("telemetry_dead_letter", json);
        continue;
    }

    var telemetry = envelope.Data;

    if (telemetry == null || !Guid.TryParse(telemetry.DeviceId, out _))
    {
        Console.WriteLine("Invalid telemetry → DLQ");

        await _redisDb.ListRightPushAsync("telemetry_dead_letter", json);
        continue;
    }

    await _repository.InsertTelemetryAsync(telemetry);

    var cacheKey = $"telemetry:{telemetry.DeviceId}";
await _redisDb.KeyDeleteAsync(cacheKey);

Console.WriteLine($"Cache invalidated ({telemetry.DeviceId})");

    Console.WriteLine($"Stored in DB from queue ({telemetry.DeviceId})");

    failureDelayMs = 1000;
}
        catch (Exception ex)
{
    Console.WriteLine($"DB Worker Error: {ex.Message}");
    Console.WriteLine("Retrying DB insert...");

    var json = value!.ToString();

    // Always retry DB failures (no retry count)
    await _redisDb.ListRightPushAsync("telemetry_queue", json);

    await Task.Delay(failureDelayMs, stoppingToken);
    failureDelayMs = Math.Min(failureDelayMs * 2, 30000);
}
    }
}
}