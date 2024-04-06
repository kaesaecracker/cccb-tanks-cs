using System.Net.WebSockets;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TanksServer;

internal sealed class ClientScreenServer(
    ILogger<ClientScreenServer> logger,
    ILoggerFactory loggerFactory,
    MapDrawer drawer
) : IHostedLifecycleService, ITickStep
{
    private readonly List<ClientScreenServerConnection> _connections = new();

    public Task HandleClient(WebSocket socket)
    {
        logger.LogDebug("HandleClient");
        var connection =
            new ClientScreenServerConnection(socket, loggerFactory.CreateLogger<ClientScreenServerConnection>(), this);
        _connections.Add(connection);
        return connection.Done;
    }

    public Task StoppingAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("closing connections");
        return Task.WhenAll(_connections.Select(c => c.CloseAsync()));
    }

    public Task TickAsync()
    {
        logger.LogTrace("Sending buffer to {} clients", _connections.Count);
        return Task.WhenAll(_connections.Select(c => c.SendAsync(drawer.LastFrame)));
    }

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StartedAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StartingAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    
    private void Remove(ClientScreenServerConnection connection) => _connections.Remove(connection);
    
    private sealed class ClientScreenServerConnection(
        WebSocket webSocket,
        ILogger<ClientScreenServerConnection> logger,
        ClientScreenServer server
    ) : EasyWebSocket(webSocket, logger, ArraySegment<byte>.Empty)
    {
        private bool _wantsNewFrame = true;

        public Task SendAsync(DisplayPixelBuffer buf)
        {
            if (!_wantsNewFrame)
                return Task.CompletedTask;
            _wantsNewFrame = false;
            return TrySendAsync(buf.Data);
        }

        protected override Task ReceiveAsync(ArraySegment<byte> buffer)
        {
            _wantsNewFrame = true;
            return Task.CompletedTask;
        }

        protected override Task ClosingAsync()
        {
            server.Remove(this);
            return Task.CompletedTask;
        }
    }
}
