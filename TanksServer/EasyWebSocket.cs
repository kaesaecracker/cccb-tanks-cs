using System.Net.WebSockets;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace TanksServer;

internal abstract class EasyWebSocket
{
    private readonly TaskCompletionSource _completionSource = new();

    private readonly ILogger _logger;
    private readonly WebSocket _socket;
    private readonly Task _readLoop;
    private readonly ArraySegment<byte> _buffer;

    protected EasyWebSocket(WebSocket socket, ILogger logger, ArraySegment<byte> buffer)
    {
        _socket = socket;
        _logger = logger;
        _buffer = buffer;
        _readLoop = ReadLoopAsync();
    }

    public Task Done => _completionSource.Task;

    private async Task ReadLoopAsync()
    {
        do
        {
            var response = await _socket.ReceiveAsync(_buffer, CancellationToken.None);
            if (response.CloseStatus.HasValue)
                break;

            await ReceiveAsync(_buffer[..response.Count]);
        } while (_socket.State == WebSocketState.Open);
    }

    protected abstract Task ReceiveAsync(ArraySegment<byte> buffer);

    protected Task SendAsync(byte[] data)
    {
        _logger.LogTrace("sending {} bytes of data", _buffer.Count);
        return _socket.SendAsync(data, WebSocketMessageType.Binary, true, CancellationToken.None);
    }

    public async Task CloseAsync()
    {
        _logger.LogDebug("closing socket");
        await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
        await _readLoop;
        _completionSource.SetResult();
    }
}
