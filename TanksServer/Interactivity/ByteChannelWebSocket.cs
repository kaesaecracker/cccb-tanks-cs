using System.Diagnostics;
using System.Net.WebSockets;
using System.Threading.Channels;

namespace TanksServer.Interactivity;

/// <summary>
/// Hacky class for easier semantics
/// </summary>
internal sealed class ByteChannelWebSocket : Channel<byte[]>
{
    private readonly ILogger _logger;
    private readonly WebSocket _socket;
    private readonly Task _backgroundDone;
    private readonly byte[] _buffer;

    private readonly Channel<byte[]> _outgoing = Channel.CreateUnbounded<byte[]>();
    private readonly Channel<byte[]> _incoming = Channel.CreateUnbounded<byte[]>();

    public ByteChannelWebSocket(WebSocket socket, ILogger logger, int messageSize)
    {
        _socket = socket;
        _logger = logger;
        _buffer = new byte[messageSize];
        _backgroundDone = Task.WhenAll(ReadLoopAsync(), WriteLoopAsync());

        Reader = _incoming.Reader;
        Writer = _outgoing.Writer;
    }

    private async Task ReadLoopAsync()
    {
        while (true)
        {
            if (_socket.State is not (WebSocketState.Open or WebSocketState.CloseSent))
                break;

            var response = await _socket.ReceiveAsync(_buffer, CancellationToken.None);
            if (response.MessageType == WebSocketMessageType.Close)
            {
                if (_socket.State == WebSocketState.CloseReceived)
                    await _socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, string.Empty,
                        CancellationToken.None);
                break;
            }

            if (response.Count != _buffer.Length)
            {
                await _socket.CloseAsync(
                    WebSocketCloseStatus.InvalidPayloadData,
                    "response has unexpected size",
                    CancellationToken.None);
                break;
            }

            await _incoming.Writer.WriteAsync(_buffer.ToArray());
        }

        if (_socket.State != WebSocketState.Closed)
            Debugger.Break();

        _incoming.Writer.Complete();
    }

    private async Task WriteLoopAsync()
    {
        await foreach (var data in _outgoing.Reader.ReadAllAsync())
        {
            _logger.LogTrace("sending {} bytes of data", data.Length);
            try
            {
                await _socket.SendAsync(data, WebSocketMessageType.Binary, true, CancellationToken.None);
            }
            catch (WebSocketException wsEx)
            {
                _logger.LogDebug(wsEx, "send failed");
            }
        }

        await _socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
    }

    public async Task CloseAsync()
    {
        _logger.LogDebug("closing socket");
        _outgoing.Writer.Complete();
        await _backgroundDone;
    }
}
