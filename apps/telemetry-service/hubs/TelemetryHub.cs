using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace TelemetryService.Hubs;

[Authorize]
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

  public override Task OnConnectedAsync()
{
    // Try multiple claim types
    var userId = Context.User?.FindFirst("sub")?.Value 
              ?? Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
              ?? Context.User?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
    
    Console.WriteLine($"User connected: {userId ?? "Unknown"}");

    return base.OnConnectedAsync();
}

public override Task OnDisconnectedAsync(Exception? exception)
{
    var userId = Context.User?.FindFirst("sub")?.Value 
              ?? Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
              ?? Context.User?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
    
    Console.WriteLine($"User disconnected: {userId ?? "Unknown"}");

    return base.OnDisconnectedAsync(exception);
}
}