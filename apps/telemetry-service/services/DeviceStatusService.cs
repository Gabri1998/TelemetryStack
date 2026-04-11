using StackExchange.Redis;

namespace TelemetryService.Services;

public class DeviceStatusService
{
    private readonly IDatabase _redis;

    public DeviceStatusService(IConnectionMultiplexer redis)
    {
        _redis = redis.GetDatabase();
    }

    public async Task<bool> IsOnlineAsync(string deviceId)
    {
        var key = $"device:lastSeen:{deviceId.ToLowerInvariant()}";

        var value = await _redis.StringGetAsync(key);

        if (!value.HasValue)
            return false;

        if (!DateTime.TryParse(value!, out var lastSeen))
            return false;

        // online if seen in last 10 seconds
        return (DateTime.UtcNow - lastSeen).TotalSeconds <= 10;
    }
}