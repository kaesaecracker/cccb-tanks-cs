using System.Net.Sockets;
using Microsoft.Extensions.Options;

namespace TanksServer;

public class ServicePointDisplay(IOptions<ServicePointDisplayConfiguration> options)
{
    private readonly UdpClient _udpClient = new(options.Value.Hostname, options.Value.Port);

    public ValueTask<int> Send(DisplayPixelBuffer buffer)
    {
        return _udpClient.SendAsync(buffer.Data);
    }
}
