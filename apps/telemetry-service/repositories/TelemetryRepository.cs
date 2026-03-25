using Npgsql;
using TelemetryService.Models;
using Microsoft.Extensions.Configuration;
namespace TelemetryService.Repositories;

public class TelemetryRepository
{
    private readonly string _connectionString;

    // Constructor: receives DB connection string
    public TelemetryRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new Exception("Postgres connection string missing");
    }
      




public async Task<List<Telemetry>> GetLatestByDeviceAsync(Guid deviceId, int limit = 50)
{
    var result = new List<Telemetry>();

    await using var conn = new NpgsqlConnection(_connectionString);
    await conn.OpenAsync();

    var cmd = new NpgsqlCommand(@"
        SELECT device_id, temperature, speed, battery, timestamp
        FROM telemetry
        WHERE device_id = @deviceId
        ORDER BY timestamp DESC
        LIMIT @limit
    ", conn);

    cmd.Parameters.AddWithValue("deviceId", deviceId);
    cmd.Parameters.AddWithValue("limit", limit);

    await using var reader = await cmd.ExecuteReaderAsync();

    while (await reader.ReadAsync())
    {
        result.Add(new Telemetry
        {
            DeviceId = reader.GetGuid(0).ToString(),
            Temperature = reader.GetDouble(1),
            Speed = reader.GetDouble(2),
            Battery = reader.GetDouble(3),
            Timestamp = reader.GetDateTime(4)
        });
    }

    return result;
}


    // Insert telemetry into database
    public async Task InsertTelemetryAsync(Telemetry telemetry)
    {
        // Create DB connection
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        // SQL query
       var cmd = new NpgsqlCommand(@"
    INSERT INTO telemetry (id, device_id, temperature, speed, battery, timestamp)
    VALUES (gen_random_uuid(), @deviceId, @temperature, @speed, @battery, @timestamp)
    ON CONFLICT (device_id, timestamp) DO NOTHING
", conn);
        // Parameters (avoid SQL injection)
        cmd.Parameters.AddWithValue("deviceId", Guid.Parse(telemetry.DeviceId));
        cmd.Parameters.AddWithValue("temperature", telemetry.Temperature);
        cmd.Parameters.AddWithValue("speed", telemetry.Speed);
        cmd.Parameters.AddWithValue("battery", telemetry.Battery);
        cmd.Parameters.AddWithValue("timestamp", telemetry.Timestamp);

        await cmd.ExecuteNonQueryAsync();
    }
}