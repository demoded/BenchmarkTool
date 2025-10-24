using Microsoft.AspNetCore.SignalR;

namespace BenchmarkTool.Web.Hubs;

/// <summary>
/// SignalR hub for real-time benchmark progress updates
/// </summary>
public class BenchmarkHub : Hub
{
    /// <summary>
    /// Send progress update to clients
    /// </summary>
    public async Task SendProgress(string message, int percentage)
    {
        await Clients.All.SendAsync("ReceiveProgress", message, percentage);
    }

    /// <summary>
    /// Send status update to clients
    /// </summary>
    public async Task SendStatus(string status)
    {
     await Clients.All.SendAsync("ReceiveStatus", status);
    }

    /// <summary>
    /// Send error to clients
    /// </summary>
    public async Task SendError(string error)
    {
        await Clients.All.SendAsync("ReceiveError", error);
    }

    /// <summary>
    /// Send results to clients
    /// </summary>
    public async Task SendResults(string results)
    {
        await Clients.All.SendAsync("ReceiveResults", results);
    }
}
