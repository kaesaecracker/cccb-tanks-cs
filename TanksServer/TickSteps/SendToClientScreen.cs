using TanksServer.Servers;
using TanksServer.Services;

namespace TanksServer.TickSteps;

internal sealed class SendToClientScreen(
    ClientScreenServer clientScreenServer,
    LastFinishedFrameProvider lastFinishedFrameProvider
) : ITickStep
{
    public Task TickAsync()
    {
        var tasks = clientScreenServer
            .GetConnections()
            .Select(c => c.SendAsync(lastFinishedFrameProvider.LastFrame));
        return Task.WhenAll(tasks);
    }
}
