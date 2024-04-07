using System.Net.Sockets;

namespace TanksServer.TickSteps;

internal sealed class SendToServicePointDisplay(
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
