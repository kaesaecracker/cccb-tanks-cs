using System.Net.WebSockets;
using Microsoft.Extensions.Logging;

namespace TanksServer.Helpers;

/// <summary>
/// Hacky class for easier semantics
/// </summary>
internal abstract class EasyWebSocket
{
    private readonly TaskCompletionSource _completionSource = new();

    private readonly ILogger _logger;
    private readonly WebSocket _socket;
    private readonly Task _readLoop;
    private readonly ArraySegment<byte> _buffer;
    private int _closed;

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
    protected abstract Task ClosingAsync();

    protected async Task TrySendAsync(byte[] data)
    {
        if (_socket.State != WebSocketState.Open)
            await CloseAsync();

        _logger.LogTrace("sending {} bytes of data", _buffer.Count);

        try
        {
            await _socket.SendAsync(data, WebSocketMessageType.Binary, true, CancellationToken.None);
        }
        catch (WebSocketException wsEx)
        {
            _logger.LogDebug(wsEx, "send failed");
        }
    }

    public async Task CloseAsync(
        WebSocketCloseStatus status = WebSocketCloseStatus.NormalClosure,
        string? description = null
    )
    {
        if (Interlocked.Exchange(ref _closed, 1) == 1)
            return;
        _logger.LogDebug("closing socket");
        await _socket.CloseAsync(status, description, CancellationToken.None);
        await _readLoop;
        await ClosingAsync();
        _completionSource.SetResult();
    }
}
