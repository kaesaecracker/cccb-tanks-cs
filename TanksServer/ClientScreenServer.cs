using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Threading.Channels;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TanksServer.Helpers;
using TanksServer.Services;

namespace TanksServer;

internal sealed class ClientScreenServer(
    ILogger<ClientScreenServer> logger,
    ILoggerFactory loggerFactory,
    PixelDrawer drawer
) : IHostedLifecycleService, ITickStep
{
    private readonly ConcurrentDictionary<ClientScreenServerConnection, byte> _connections = new();
    private bool _closing;

    public Task HandleClient(WebSocket socket)
    {
        if (_closing)
        {
            logger.LogWarning("ignoring request because connections are closing");
            return Task.CompletedTask;
        }

        logger.LogDebug("HandleClient");
        var connection =
            new ClientScreenServerConnection(socket, loggerFactory.CreateLogger<ClientScreenServerConnection>(), this);
        var added = _connections.TryAdd(connection, 0);
        Debug.Assert(added);
        return connection.Done;
    }

    public Task StoppingAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("closing connections");
        _closing = true;
        return Task.WhenAll(_connections.Keys.Select(c => c.CloseAsync()));
    }

    public Task TickAsync()
    {
        logger.LogTrace("Sending buffer to {} clients", _connections.Count);
        return Task.WhenAll(_connections.Keys.Select(c => c.SendAsync(drawer.LastFrame)));
    }

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StartedAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StartingAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private void Remove(ClientScreenServerConnection connection) => _connections.TryRemove(connection, out _);

    private sealed class ClientScreenServerConnection: IDisposable
    {
        private readonly ByteChannelWebSocket _channel;
        private readonly SemaphoreSlim _wantedFrames = new(1);
        private readonly ClientScreenServer _server;
        private readonly ILogger<ClientScreenServerConnection> _logger;

        public ClientScreenServerConnection(WebSocket webSocket,
            ILogger<ClientScreenServerConnection> logger,
            ClientScreenServer server)
        {
            _server = server;
            _logger = logger;
            _channel = new(webSocket, logger, 0);
            Done = ReceiveAsync();
        }

        public async Task SendAsync(DisplayPixelBuffer buf)
        {
            if (await _wantedFrames.WaitAsync(TimeSpan.Zero))
            {
                _logger.LogTrace("sending");
                try
                {
                    await _channel.Writer.WriteAsync(buf.Data);
                }
                catch (ChannelClosedException)
                {
                    _logger.LogWarning("send failed, channel is closed");
                }
            }
            else
            {
                _logger.LogTrace("client does not want a frame yet");
            }
        }

        private async Task ReceiveAsync()
        {
            await foreach (var _ in _channel.Reader.ReadAllAsync()) 
                _wantedFrames.Release();
            
            _logger.LogTrace("done receiving");
            _server.Remove(this);
        }

        public Task CloseAsync()
        {
            _logger.LogDebug("closing connection");
            return _channel.CloseAsync();
        }

        public Task Done { get; }

        public void Dispose()
        {
            _wantedFrames.Dispose();
            Done.Dispose();
        }
    }
}
