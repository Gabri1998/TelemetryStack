using System.Text.Json;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;
using TelemetryService.Models;
using TelemetryService.Repositories;

namespace TelemetryService.Services;

public class TelemetryDbWorker : BackgroundService
{
    private readonly IDatabase _redisDb;
    private readonly IServiceScopeFactory _scopeFactory;

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public TelemetryDbWorker(
        IConnectionMultiplexer redis,
        IServiceScopeFactory scopeFactory)
    {
        _redisDb = redis.GetDatabase();
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("DB Worker started");

        int failureDelayMs = 1000;

        while (!stoppingToken.IsCancellationRequested)
        {
           var value = await _redisDb.ListRightPopLeftPushAsync(
                "telemetry_queue",
                "telemetry_processing"
            );

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

                            // remove from processing queue
                        await _redisDb.ListRemoveAsync("telemetry_processing", value,1);

                        continue;
                }

                var telemetry = envelope.Data;

                if (telemetry == null || telemetry.DeviceId == Guid.Empty)
                {
                    Console.WriteLine("Invalid telemetry → DLQ");
                    await _redisDb.ListRightPushAsync("telemetry_dead_letter", json);

                    // remove from processing queue
                    await _redisDb.ListRemoveAsync("telemetry_processing", value,1);

                    continue;
                }

                //  create scope here
                using var scope = _scopeFactory.CreateScope();
                var repo = scope.ServiceProvider.GetRequiredService<TelemetryRepository>();
                
                await repo.InsertTelemetryAsync(telemetry);
                await _redisDb.ListRemoveAsync("telemetry_processing", value,1);
                var cacheKey = $"telemetry:{telemetry.DeviceId}";
                await _redisDb.KeyDeleteAsync(cacheKey);

                Console.WriteLine($"Stored in DB ({telemetry.DeviceId})");

                failureDelayMs = 1000;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DB Worker Error: {ex.Message}");

              var json = value!.ToString();

                //  remove from processing first
                await _redisDb.ListRemoveAsync("telemetry_processing", value,1);

                //  requeue for retry
                await _redisDb.ListRightPushAsync("telemetry_queue", json);

                await Task.Delay(failureDelayMs, stoppingToken);
                failureDelayMs = Math.Min(failureDelayMs * 2, 30000);
            }
        }
    }
}