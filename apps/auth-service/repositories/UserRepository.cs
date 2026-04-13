using Npgsql;
using AuthService.Models;
using Microsoft.Extensions.Configuration;

namespace AuthService.Repositories;

public class UserRepository
{
    private readonly string _connectionString;

    public UserRepository(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("Postgres")
            ?? throw new Exception("Missing connection string");
    }

    public async Task CreateAsync(User user)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        var cmd = new NpgsqlCommand(@"
            INSERT INTO users (id, email, password_hash)
            VALUES (@id, @email, @password)
        ", conn);

        cmd.Parameters.AddWithValue("id", user.Id);
        cmd.Parameters.AddWithValue("email", user.Email);
        cmd.Parameters.AddWithValue("password", user.PasswordHash);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        var cmd = new NpgsqlCommand(@"
            SELECT id, email, password_hash
            FROM users
            WHERE email = @email
        ", conn);

        cmd.Parameters.AddWithValue("email", email);

        await using var reader = await cmd.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return new User
            {
                Id = reader.GetGuid(0),
                Email = reader.GetString(1),
                PasswordHash = reader.GetString(2)
            };
        }

        return null;
    }
}