using System.Diagnostics;
using System.Net.WebSockets;
using DisplayCommands;
using Microsoft.Extensions.Hosting;
using TanksServer.Graphics;

namespace TanksServer.Interactivity;

internal sealed class ClientScreenServer(
    ILogger<ClientScreenServer> logger,
    ILoggerFactory loggerFactory
) : IHostedLifecycleService, IFrameConsumer
{
    private readonly ConcurrentDictionary<ClientScreenServerConnection, byte> _connections = new();
    private bool _closing;

    public Task StoppingAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("closing connections");
        _closing = true;
        return Task.WhenAll(_connections.Keys.Select(c => c.CloseAsync()));
    }

    public Task HandleClient(WebSocket socket, Guid? playerGuid)
    {
        if (_closing)
        {
            logger.LogWarning("ignoring request because connections are closing");
            return Task.CompletedTask;
        }

        logger.LogDebug("HandleClient");
        var connection = new ClientScreenServerConnection(
            socket,
            loggerFactory.CreateLogger<ClientScreenServerConnection>(),
            this,
            playerGuid);
        var added = _connections.TryAdd(connection, 0);
        Debug.Assert(added);
        return connection.Done;
    }

    public void Remove(ClientScreenServerConnection connection) => _connections.TryRemove(connection, out _);

    public IEnumerable<ClientScreenServerConnection> GetConnections() => _connections.Keys;


    public Task OnFrameDoneAsync(GamePixelGrid gamePixelGrid, PixelGrid observerPixels)
    {
        var tasks = _connections.Keys
            .Select(c => c.SendAsync(observerPixels, gamePixelGrid));
        return Task.WhenAll(tasks);
    }

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StartedAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StartingAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
