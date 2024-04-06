using System.Net.WebSockets;

namespace TanksServer;

internal sealed class ClientScreenServer
{
    private readonly List<ClientScreenServerConnection> _connections = new();

    public ClientScreenServerConnection AddClient(WebSocket socket)
    {
        var connection = new ClientScreenServerConnection(socket);
        _connections.Add(connection);
        return connection;
    }

    public Task Send(DisplayPixelBuffer buf)
    {
        return Task.WhenAll(_connections.Select(c => c.Send(buf)));
    }
}

internal sealed class ClientScreenServerConnection
{
    private readonly WebSocket _socket;
    private readonly Task _readTask;
    private readonly TaskCompletionSource _completionSource = new();
    private bool _wantsNewFrame = true;

    public ClientScreenServerConnection(WebSocket webSocket)
    {
        _socket = webSocket;
        _readTask = Read();
    }

    public Task Done => _completionSource.Task;

    private async Task Read()
    {
        while (true)
        {
            await _socket.ReceiveAsync(ArraySegment<byte>.Empty, default);
            _wantsNewFrame = true;
        }
    }

    public Task Send(DisplayPixelBuffer buf)
    {
        if (!_wantsNewFrame)
            return Task.CompletedTask;
        return _socket.SendAsync(buf.Data, WebSocketMessageType.Binary, true, default);
    }
}
