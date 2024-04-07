using System.Net.Sockets;
using TanksServer.Models;
using TanksServer.Services;

namespace TanksServer.TickSteps;

internal sealed class SendToServicePointDisplay(
    IOptions<ServicePointDisplayConfiguration> options,
    LastFinishedFrameProvider lastFinishedFrameProvider
) : ITickStep, IDisposable
{
    private readonly UdpClient? _udpClient = options.Value.Enable
        ? new(options.Value.Hostname, options.Value.Port)
        : null;

    public Task TickAsync()
    {
        return _udpClient?.SendAsync(lastFinishedFrameProvider.LastFrame.Data).AsTask() ?? Task.CompletedTask;
    }

    public void Dispose()
    {
        _udpClient?.Dispose();
    }
}
