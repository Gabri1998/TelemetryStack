using Microsoft.AspNetCore.SignalR;

namespace TelemetryService.Hubs;

public class TelemetryHub : Hub
{
    public async Task JoinDevice(string deviceId)
    {
        var group = deviceId.ToLowerInvariant();

        await Groups.AddToGroupAsync(Context.ConnectionId, group);

        Console.WriteLine($"Client {Context.ConnectionId} joined {group}");
    }

    public async Task LeaveDevice(string deviceId)
    {
        var group = deviceId.ToLowerInvariant();

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, group);

        Console.WriteLine($"Client {Context.ConnectionId} left {group}");
    }
}