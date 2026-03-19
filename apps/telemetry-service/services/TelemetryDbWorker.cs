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

        while (!stoppingToken.IsCancellationRequested)
        {
            var value = await _redisDb.ListLeftPopAsync("telemetry_queue");

            if (value.IsNullOrEmpty)
            {
                await Task.Delay(500, stoppingToken);
                continue;
            }

            try
            {
                var json = value!.ToString();

                var telemetry = JsonSerializer.Deserialize<Telemetry>(json, _jsonOptions);

                if (telemetry == null)
                {
                    Console.WriteLine("Invalid telemetry in queue");
                    continue;
                }

                await _repository.InsertTelemetryAsync(telemetry);

                Console.WriteLine($"Stored in DB from queue ({telemetry.DeviceId})");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DB Worker Error: {ex.Message}");

                // push back to queue to avoid data loss
                await _redisDb.ListLeftPushAsync("telemetry_queue", value);

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}