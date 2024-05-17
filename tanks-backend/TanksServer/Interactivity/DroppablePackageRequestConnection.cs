using DotNext.Threading;

namespace TanksServer.Interactivity;

internal abstract class DroppablePackageRequestConnection<TPackage>(
    ILogger logger,
    ByteChannelWebSocket socket
) : WebsocketServerConnection(logger, socket), IDisposable
    where TPackage : class, IDisposable
{
    private readonly AsyncAutoResetEvent _nextPackageEvent = new(false, 1);
    private TPackage? _next;

    protected override async ValueTask HandleMessageAsync(Memory<byte> _)
    {
        await _nextPackageEvent.WaitAsync();
        var package = Interlocked.Exchange(ref _next, null);
        if (package == null)
            return;
        await SendPackageAsync(package);
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
