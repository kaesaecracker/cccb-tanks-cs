using System.Net.WebSockets;
using System.Threading;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TanksServer;

internal sealed class ClientScreenServer(
    ILogger<ClientScreenServer> logger,
    ILoggerFactory loggerFactory
) : IHostedLifecycleService
{
    private readonly List<ClientScreenServerConnection> _connections = new();

    public Task HandleClient(WebSocket socket)
    {
        logger.LogDebug("HandleClient");
        var connection =
            new ClientScreenServerConnection(socket, loggerFactory.CreateLogger<ClientScreenServerConnection>());
        _connections.Add(connection);
        return connection.Done;
    }

    public Task Send(DisplayPixelBuffer buf)
    {
        logger.LogDebug("Sending buffer to {} clients", _connections.Count);
        return Task.WhenAll(_connections.Select(c => c.Send(buf)));
    }

    public Task StoppingAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("closing connections");
        return Task.WhenAll(_connections.Select(c => c.CloseAsync()));
    }

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StartedAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StartingAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

internal sealed class ClientScreenServerConnection(WebSocket webSocket, ILogger<ClientScreenServerConnection> logger)
    : EasyWebSocket(webSocket, logger, ArraySegment<byte>.Empty)
{
    private bool _wantsNewFrame = true;

    public Task Send(DisplayPixelBuffer buf)
    {
        if (!_wantsNewFrame)
            return Task.CompletedTask;
        return SendAsync(buf.Data);
    }

    protected override Task ReceiveAsync(ArraySegment<byte> buffer)
    {
        _wantsNewFrame = true;
        return Task.CompletedTask;
    }
}
