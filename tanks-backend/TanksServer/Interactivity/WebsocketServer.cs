using System.Diagnostics;
using Microsoft.Extensions.Hosting;

namespace TanksServer.Interactivity;

internal abstract class WebsocketServer<T>(
    ILogger logger
) : IHostedLifecycleService
    where T : WebsocketServerConnection
{
    private bool _closing;
    private readonly ConcurrentDictionary<T, byte> _connections = [];

    public async Task StoppingAsync(CancellationToken cancellationToken)
    {
        _closing = true;
        logger.LogInformation("closing connections");
        await _connections.Keys.Select(c => c.CloseAsync())
            .WhenAll();
        logger.LogInformation("closed connections");
    }

    protected IEnumerable<T> Connections => _connections.Keys;

    protected async Task HandleClientAsync(T connection)
    {
        if (_closing)
        {
            logger.LogWarning("refusing connection because server is shutting down");
            await connection.CloseAsync();
            return;
        }

        var added = _connections.TryAdd(connection, 0);
        Debug.Assert(added);

        await connection.ReceiveAsync();

        _ = _connections.TryRemove(connection, out _);
        await connection.RemovedAsync();
    }

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StartedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StartingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
