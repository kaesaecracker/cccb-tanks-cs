using System.Diagnostics;
using DotNext.Threading;

namespace TanksServer.Interactivity;

internal abstract class DroppablePackageRequestConnection<TPackage>(
    ILogger logger,
    ByteChannelWebSocket socket
) : WebsocketServerConnection(logger, socket), IDisposable
    where TPackage : class, IDisposable
{
    private readonly AsyncAutoResetEvent _nextPackageEvent = new(false, 1);
    private int _runningMessageHandlers = 0;
    private TPackage? _next;

    protected override ValueTask HandleMessageAsync(Memory<byte> _)
    {
        if (Interlocked.Increment(ref _runningMessageHandlers) == 1)
            return Core();

        // client has requested multiple frames, ignoring duplicate requests
        Interlocked.Decrement(ref _runningMessageHandlers);
        return ValueTask.CompletedTask;

        async ValueTask Core()
        {
            await _nextPackageEvent.WaitAsync();
            var package = Interlocked.Exchange(ref _next, null);
            if (package == null)
                throw new UnreachableException("package should be set here");
            await SendPackageAsync(package);
            Interlocked.Decrement(ref _runningMessageHandlers);
        }
    }

    protected void SetNextPackage(TPackage next)
    {
        var oldNext = Interlocked.Exchange(ref _next, next);
        _nextPackageEvent.Set();
        oldNext?.Dispose();
    }

    protected abstract ValueTask SendPackageAsync(TPackage package);

    public override void Dispose()
    {
        _nextPackageEvent.Dispose();
        Interlocked.Exchange(ref _next, null)?.Dispose();
    }
}
