namespace TanksServer.Interactivity;

internal abstract class WebsocketServerConnection(
    ILogger logger,
    ByteChannelWebSocket socket
) : IDisposable
{
    private readonly SemaphoreSlim _mutex = new(1);
    protected readonly ByteChannelWebSocket Socket = socket;
    protected readonly ILogger Logger = logger;

    public Task CloseAsync()
    {
        Logger.LogDebug("closing connection");
        return Socket.CloseAsync();
    }

    public async Task ReceiveAsync()
    {
        await foreach (var buffer in Socket.ReadAllAsync())
            await HandleMessageAsync(buffer);
        Logger.LogTrace("done receiving");
    }

    protected virtual ValueTask HandleMessageAsync(Memory<byte> buffer)
        => LockedAsync(() => HandleMessageLockedAsync(buffer));

    protected abstract ValueTask HandleMessageLockedAsync(Memory<byte> buffer);

    protected async ValueTask LockedAsync(Func<ValueTask> action)
    {
        await _mutex.WaitAsync();
        try
        {
            await action();
        }
        finally
        {
            _mutex.Release();
        }
    }

    public void Dispose() => _mutex.Dispose();
}
