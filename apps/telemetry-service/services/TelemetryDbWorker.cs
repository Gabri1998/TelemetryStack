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
    bool dbHealthy = true;

    while (!stoppingToken.IsCancellationRequested)
    {
        // If DB is down → pause consumption entirely
        if (!dbHealthy)
        {
            Console.WriteLine("DB unavailable, pausing consumption...");
            await Task.Delay(failureDelayMs, stoppingToken);
        }

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
                Console.WriteLine("Invalid envelope");
                continue;
            }

            var telemetry = envelope.Data;

            if (telemetry == null)
            {
                Console.WriteLine("Invalid telemetry in queue");
                continue;
            }

            await _repository.InsertTelemetryAsync(telemetry);

            Console.WriteLine($"Stored in DB from queue ({telemetry.DeviceId})");

            // DB is healthy again
            dbHealthy = true;
            failureDelayMs = 1000;
        }
        catch (Exception ex)
{
    Console.WriteLine($"DB Worker Error: {ex.Message}");

    var json = value!.ToString();
    var envelope = JsonSerializer.Deserialize<QueueItem>(json, _jsonOptions);

    if (envelope == null)
    {
        Console.WriteLine("Invalid envelope → DLQ");

        await _redisDb.ListRightPushAsync("telemetry_dead_letter", json);
        continue;
    }
    
    //  TEMPORARY DB ERRORS (DO NOT COUNT RETRIES)
    if (ex is Npgsql.NpgsqlException || ex is TimeoutException)
    {
        await _redisDb.ListRightPushAsync("telemetry_queue", json);

        await Task.Delay(failureDelayMs, stoppingToken);
        failureDelayMs = Math.Min(failureDelayMs * 2, 30000);

        continue;
    }

    // PERMANENT ERRORS ONLY
    envelope.RetryCount++;

    if (envelope.RetryCount > 5)
    {
        Console.WriteLine("Moved to dead letter queue");

        await _redisDb.ListRightPushAsync(
            "telemetry_dead_letter",
            JsonSerializer.Serialize(envelope)
        );

        continue;
    }

    await _redisDb.ListRightPushAsync(
        "telemetry_queue",
        JsonSerializer.Serialize(envelope)
    );
}
    }
}
}