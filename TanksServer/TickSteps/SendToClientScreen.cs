using TanksServer.Servers;

namespace TanksServer.TickSteps;

internal sealed class SendToClientScreen(
    ClientScreenServer clientScreenServer, PixelDrawer drawer
) : ITickStep
{
    public Task TickAsync()
    {
        return Task.WhenAll(clientScreenServer.GetConnections().Select(c => c.SendAsync(drawer.LastFrame)));
    }
}
