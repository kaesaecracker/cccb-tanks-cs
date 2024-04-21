namespace TanksServer.Interactivity;

internal abstract class WebsocketServerConnection(
    ILogger logger,
    ByteChannelWebSocket socket)
{
    protected readonly ByteChannelWebSocket Socket = socket;

    public Task CloseAsync()
    {
        logger.LogDebug("closing connection");
        return Socket.CloseAsync();
    }

    public async Task ReceiveAsync()
    {
        await foreach (var buffer in Socket.ReadAllAsync())
            HandleMessage(buffer);
        logger.LogTrace("done receiving");
    }

    protected abstract void HandleMessage(Memory<byte> buffer);
}
