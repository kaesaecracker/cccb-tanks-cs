using System.Net.Sockets;
using Microsoft.Extensions.Options;

namespace TanksServer.Services;

internal sealed class ServicePointDisplay(
    IOptions<ServicePointDisplayConfiguration> options, 
    PixelDrawer drawer
) : ITickStep, IDisposable
{
    private readonly UdpClient _udpClient = new(options.Value.Hostname, options.Value.Port);

    public Task TickAsync()
    {
        return _udpClient.SendAsync(drawer.LastFrame.Data).AsTask();
    }

    public void Dispose()
    {
        _udpClient.Dispose();
    }
}

internal sealed class ServicePointDisplayConfiguration
{
    public string Hostname { get; set; } = string.Empty;
    public int Port { get; set; }
}
