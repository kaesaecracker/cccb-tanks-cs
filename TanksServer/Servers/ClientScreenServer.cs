using System.Diagnostics;
using System.Net.WebSockets;
using System.Threading.Channels;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TanksServer.Helpers;
using TanksServer.ServicePointDisplay;

namespace TanksServer.Servers;

internal sealed class ClientScreenServer(
    ILogger<ClientScreenServer> logger,
    ILoggerFactory loggerFactory
) : IHostedLifecycleService
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

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StartedAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StartingAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private void Remove(ClientScreenServerConnection connection) => _connections.TryRemove(connection, out _);
    
    public IEnumerable<ClientScreenServerConnection> GetConnections() => _connections.Keys;

    internal sealed class ClientScreenServerConnection: IDisposable
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

        public async Task SendAsync(PixelDisplayBufferView buf)
        {
            if (!await _wantedFrames.WaitAsync(TimeSpan.Zero))
            {
                _logger.LogTrace("client does not want a frame yet");
                return;
            }

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
