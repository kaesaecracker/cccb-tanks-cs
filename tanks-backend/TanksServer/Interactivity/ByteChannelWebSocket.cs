using System.Diagnostics;
using System.Net.WebSockets;

namespace TanksServer.Interactivity;

internal sealed class ByteChannelWebSocket(WebSocket socket, ILogger logger, int messageSize)
{
    private readonly byte[] _buffer = new byte[messageSize];

    public async ValueTask SendBinaryAsync(ReadOnlyMemory<byte> data, bool endOfMessage = true)
    {
        try
        {
            await socket.SendAsync(data, WebSocketMessageType.Binary, endOfMessage, CancellationToken.None);
        }
        catch (WebSocketException e)
        {
            logger.LogError(e, "could not send binary message");
            await CloseAsync();
        }
    }

    public async ValueTask SendTextAsync(ReadOnlyMemory<byte> data, bool endOfMessage = true)
    {
        try
        {
            await socket.SendAsync(data, WebSocketMessageType.Text, endOfMessage, CancellationToken.None);
        }
        catch (WebSocketException e)
        {
            logger.LogError(e, "could not send text message");
            await CloseAsync();
        }
    }

    public async IAsyncEnumerable<Memory<byte>> ReadAllAsync()
    {
        while (socket.State is WebSocketState.Open or WebSocketState.CloseSent)
        {
            if (await TryReadAsync())
                yield return _buffer.ToArray();
        }

        if (socket.State is not WebSocketState.Closed and not WebSocketState.Aborted)
            Debugger.Break();
    }

    public Task CloseWithErrorAsync(string error)
        => socket.CloseOutputAsync(WebSocketCloseStatus.InternalServerError, error, CancellationToken.None);

    public async Task CloseAsync()
    {
        if (socket.State is not WebSocketState.Open and not WebSocketState.CloseReceived)
            return;

        try
        {
            await socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
        }
        catch (WebSocketException socketException)
        {
            logger.LogDebug(socketException, "could not close socket properly");
        }
    }

    private async Task<bool> TryReadAsync()
    {
        try
        {
            var response = await socket.ReceiveAsync(_buffer, CancellationToken.None);
            if (response.MessageType == WebSocketMessageType.Close)
            {
                await socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, string.Empty,
                    CancellationToken.None);
                return false;
            }

            if (response.Count != _buffer.Length)
            {
                await socket.CloseOutputAsync(WebSocketCloseStatus.InvalidPayloadData,
                    "response has unexpected size",
                    CancellationToken.None);
                return false;
            }

            return true;
        }
        catch (WebSocketException socketException)
        {
            logger.LogDebug(socketException, "could not read");
            return false;
        }
    }
}
