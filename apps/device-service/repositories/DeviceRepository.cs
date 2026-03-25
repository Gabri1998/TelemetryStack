using DeviceService.Models;
using Npgsql;

namespace DeviceService.Repositories;

public class DeviceRepository
{
    private readonly string _connectionString;

   public DeviceRepository(IConfiguration configuration)
{
    Console.WriteLine("==== CONFIG DEBUG ====");

    foreach (var kv in configuration.AsEnumerable())
    {
        Console.WriteLine($"{kv.Key} = {kv.Value}");
    }

    var conn = configuration.GetConnectionString("DefaultConnection");

    Console.WriteLine($"CONNECTION STRING: {conn}");

    _connectionString = conn
        ?? throw new Exception("DB connection missing");
}
    public List<Device> GetDevices()
    {
        var devices = new List<Device>();

        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();

        var cmd = new NpgsqlCommand("SELECT id, name FROM devices", conn);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            devices.Add(new Device
            {
                Id = reader.GetGuid(0).ToString(),
                Name = reader.GetString(1)
            });
        }

        return devices;
    }

    public void AddDevice(Device device)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();

        var cmd = new NpgsqlCommand(
            "INSERT INTO devices (id, name) VALUES (@id, @name)",
            conn
        );

        cmd.Parameters.AddWithValue("id", Guid.Parse(device.Id));
        cmd.Parameters.AddWithValue("name", device.Name);

        cmd.ExecuteNonQuery();
    }
}