using System.Diagnostics;
using System.Net.WebSockets;

namespace TanksServer.Interactivity;

internal sealed class ByteChannelWebSocket(WebSocket socket, ILogger logger, int messageSize)
{
    private readonly byte[] _buffer = new byte[messageSize];

    public ValueTask SendAsync(ReadOnlyMemory<byte> data) =>
        socket.SendAsync(data, WebSocketMessageType.Binary, true, CancellationToken.None);

    public async IAsyncEnumerable<Memory<byte>> ReadAllAsync()
    {
        while (true)
        {
            if (socket.State is not (WebSocketState.Open or WebSocketState.CloseSent))
                break;

            var response = await socket.ReceiveAsync(_buffer, CancellationToken.None);
            if (response.MessageType == WebSocketMessageType.Close)
            {
                if (socket.State == WebSocketState.CloseReceived)
                    await socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, string.Empty,
                        CancellationToken.None);
                break;
            }

            if (response.Count != _buffer.Length)
            {
                await socket.CloseOutputAsync(
                    WebSocketCloseStatus.InvalidPayloadData,
                    "response has unexpected size",
                    CancellationToken.None);
                break;
            }

            yield return _buffer.ToArray();
        }

        if (socket.State != WebSocketState.Closed)
            Debugger.Break();
    }

    public async Task CloseAsync()
    {
        logger.LogDebug("closing socket");
        await socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
    }
}