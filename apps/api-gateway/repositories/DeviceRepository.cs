using ApiGateway.Models;
using Npgsql;

namespace ApiGateway.Repositories;

// Repository handles data access (database operations)
public class DeviceRepository
{
    private readonly string _connectionString;

    // Constructor receives configuration (Dependency Injection later)
    public DeviceRepository(IConfiguration configuration)
{
    _connectionString = configuration.GetConnectionString("DefaultConnection")
        ?? throw new Exception("Database connection string is missing");
}

    // Get all devices from database
    public List<Device> GetDevices()
    {
        var devices = new List<Device>();

        // Create database connection
        using var connection = new NpgsqlConnection(_connectionString);

        // Open connection
        connection.Open();

        // SQL query
        var query = "SELECT id, name FROM devices";

        using var command = new NpgsqlCommand(query, connection);

        using var reader = command.ExecuteReader();

        // Read rows from database
        while (reader.Read())
        {
            var device = new Device
            {
                Id = reader.GetGuid(0).ToString(),
                Name = reader.GetString(1)
            };

            devices.Add(device);
        }

        return devices;
    }


        public void AddDevice(Device device)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        var query = "INSERT INTO devices (id, name) VALUES (@id, @name)";

        using var command = new NpgsqlCommand(query, connection);

        command.Parameters.AddWithValue("id", Guid.Parse(device.Id));
        command.Parameters.AddWithValue("name", device.Name);

        command.ExecuteNonQuery();
    }

}